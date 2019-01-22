using System;
using System.IO;
using System.Text;

namespace Icembler
{
    public class VirtualDisk
    {
        // Disk size = 256 bytes of data per sector x 18 sectors per track x 35 tracks
        //             256 x 18 x 35 = 161,280
        private const int DiskSize = 161280;
        private readonly byte[] _virtualDisk;
        private readonly byte[] _gatTrack;
        private int _fileNumber;
        private byte _currentGranule;
        private byte _nextGranule;

        public VirtualDisk()
        {
            _fileNumber = 0;
            _currentGranule = 0;
            _nextGranule = 0;
            _virtualDisk = new byte[DiskSize];
            _gatTrack = new byte[4608];

            for (int i = 0; i < _virtualDisk.Length; i++)
            {
                _virtualDisk[i] = 0xff;
            }
        }

        public byte[] ConvertToDecbMachineLanguageFile(byte[] program, ushort startAddress, ushort execAddress)
        {
            byte[] retval = new byte[program.Length + 10];

            retval[0] = 0x00;
            retval[1] = (byte) ((program.Length >> 8) & 0xFFu);
            retval[2] = (byte) (program.Length & 0xFFu);
            retval[3] = (byte) ((startAddress >> 8) & 0xFFu);
            retval[4] = (byte) (startAddress & 0xFFu);

            Array.Copy(program, 0, retval, 5, program.Length);

            retval[program.Length + 5] = 0xff;
            retval[program.Length + 6] = 0x00;
            retval[program.Length + 7] = 0x00;
            retval[program.Length + 8] = (byte) ((execAddress >> 8) & 0xFFu);
            retval[program.Length + 9] = (byte) (execAddress & 0xFFu);

            return retval;
        }

        public void AddFile(byte[] fileBytes, string cocoFileName, FileTypes fileType = FileTypes.Basic, FileModes fileMode = FileModes.Automatic)
        {
            // One track has 2 granules, so a 34 track disk has 34 x 2 = 68
            const int granulesPerDisk = 68;
            // GAT Table starts on track 17. This would be
            //     256 x 18 x 17 = 78336 bytes in
            const int gatStartingPosition = 78336;
            // GAT position inside the GAT track
            const int gatTrackPosition = 256;
            // Directory position inside the GAT track
            const int gatDirectoryTrackPosition = 512;

            byte firstGranule = 0;

            byte[] gatByteOrder =
            {
                0x20, 0x21, 0x22, 0x23, 0x1e, 0x1f, 0x24, 0x25, 0x1c, 0x1d, 0x26, 0x27, 0x1a, 0x1b, 0x28, 0x29, 0x18,
                0x19, 0x2a, 0x2b, 0x16, 0x17, 0x2c, 0x2d, 0x14, 0x15, 0x2e, 0x2f, 0x12, 0x13, 0x30, 0x31, 0x10, 0x11,
                0x32, 0x33, 0x0e, 0x0f, 0x34, 0x35, 0x0c, 0x0d, 0x36, 0x37, 0x0a, 0x0b, 0x38, 0x39, 0x08, 0x09, 0x3a,
                0x3b, 0x06, 0x07, 0x3c, 0x3d, 0x04, 0x05, 0x3e, 0x3f, 0x02, 0x03, 0x40, 0x41, 0x00, 0x01, 0x42, 0x43
            };

            if (fileMode == FileModes.Automatic)
            {
                fileMode = fileType == FileTypes.Basic || fileType == FileTypes.Text || fileType == FileTypes.Data
                    ? FileModes.Ascii
                    : FileModes.Binary;
            }

            if (fileBytes.Length > (gatByteOrder.Length - _currentGranule) * 0x900)
            {
                throw new Exceptions.VirtualDiskFullException("Virtual Disk is Full");
            }

            string name = Path.GetFileNameWithoutExtension(cocoFileName)?.PadRight(8, ' ');
            string ext = Path.GetExtension(cocoFileName);

            ext = string.IsNullOrEmpty(ext) ? "   " : ext.Remove(0, 1).PadRight(3, ' ');

            if (name?.Length > 8)
            {
                throw new Exceptions.FileNameTooLongException("The coco file name must be 8 characters or less");
            }

            if (_fileNumber == 0)
            {
                for (int i = 0; i < _virtualDisk.Length; i++)
                {
                    _virtualDisk[i] = 0xff;
                }

                for (int i = gatTrackPosition; i < gatTrackPosition + 0x100; i++)
                {
                    _gatTrack[i] = 0;
                }

                for (int i = gatTrackPosition; i < gatTrackPosition + granulesPerDisk; i++)
                {
                    _gatTrack[i] = 0xff;
                }
            }

            firstGranule = _nextGranule;
            int bytesRead = 0;
            using (var reader = new BinaryReader(new MemoryStream(fileBytes)) )
            {
                while (_nextGranule < gatByteOrder.Length)
                {
                    _currentGranule = _nextGranule;
                    byte[] data = reader.ReadBytes(0x900);
                    bytesRead = data.Length;
                    if (data.Length == 0) break;
                    Array.Copy(data, 0, _virtualDisk, gatByteOrder[_nextGranule] * 0x900, data.Length);
                    _nextGranule++;
                    _gatTrack[0x100 + gatByteOrder[_currentGranule]] = (byte)(data.Length == 0x900 ? gatByteOrder[_nextGranule] : 0xc1 + data.Length / 0x100);
                    if (bytesRead < 0x900) break;
                }
            }

            for (int i = gatDirectoryTrackPosition + _fileNumber * 0x20; i < gatDirectoryTrackPosition + _fileNumber * 0x20 + 0x20; i++)
            {
                _gatTrack[i] = 0;
            }

            for (int i = 0; i < 11; i++)
            {
                _gatTrack[gatDirectoryTrackPosition + _fileNumber * 0x20 + i] = Encoding.ASCII.GetBytes(name?.ToUpper() + ext.ToUpper())[i];
            }

            _gatTrack[gatDirectoryTrackPosition + _fileNumber * 0x20 + 0x0b] = (byte)fileType;
            _gatTrack[gatDirectoryTrackPosition + _fileNumber * 0x20 + 0x0c] = (byte)fileMode;
            _gatTrack[gatDirectoryTrackPosition + _fileNumber * 0x20 + 0x0d] = gatByteOrder[firstGranule];
            _gatTrack[gatDirectoryTrackPosition + _fileNumber * 0x20 + 0x0e] = (byte)(bytesRead != 0 && bytesRead % 0x100 == 0 ? 0x01 : 0x00);
            _gatTrack[gatDirectoryTrackPosition + _fileNumber * 0x20 + 0x0f] = (byte)(bytesRead % 0x100);

            Array.Copy(_virtualDisk, gatStartingPosition, _virtualDisk, gatStartingPosition + 0x900 * 2, _virtualDisk.Length - gatStartingPosition - 0x900 * 2);
            Array.Copy(_gatTrack, 0, _virtualDisk, gatStartingPosition, _gatTrack.Length);

            _fileNumber++;
        }

        public void AddFile(string fileName, string cocoFileName, FileTypes fileType = FileTypes.Basic, FileModes fileMode = FileModes.Automatic)
        {
            // One track has 2 granules, so a 34 track disk has 34 x 2 = 68
            const int granulesPerDisk = 68;
            // GAT Table starts on track 17. This would be
            //     256 x 18 x 17 = 78336 bytes in
            const int gatStartingPosition = 78336;
            // GAT position inside the GAT track
            const int gatTrackPosition = 256;
            // Directory position inside the GAT track
            const int gatDirectoryTrackPosition = 512;

            byte firstGranule = 0;

            byte[] gatByteOrder =
            {
                0x20, 0x21, 0x22, 0x23, 0x1e, 0x1f, 0x24, 0x25, 0x1c, 0x1d, 0x26, 0x27, 0x1a, 0x1b, 0x28, 0x29, 0x18,
                0x19, 0x2a, 0x2b, 0x16, 0x17, 0x2c, 0x2d, 0x14, 0x15, 0x2e, 0x2f, 0x12, 0x13, 0x30, 0x31, 0x10, 0x11,
                0x32, 0x33, 0x0e, 0x0f, 0x34, 0x35, 0x0c, 0x0d, 0x36, 0x37, 0x0a, 0x0b, 0x38, 0x39, 0x08, 0x09, 0x3a,
                0x3b, 0x06, 0x07, 0x3c, 0x3d, 0x04, 0x05, 0x3e, 0x3f, 0x02, 0x03, 0x40, 0x41, 0x00, 0x01, 0x42, 0x43
            };

            byte[] fileArray = File.ReadAllBytes(fileName);

            if (fileMode == FileModes.Automatic)
            {
                fileMode = fileType == FileTypes.Basic || fileType == FileTypes.Text || fileType == FileTypes.Data
                    ? FileModes.Ascii
                    : FileModes.Binary;
            }

            if (fileArray.Length > (gatByteOrder.Length - _currentGranule) * 0x900)
            {
                throw new Exceptions.VirtualDiskFullException("Virtual Disk is Full");
            }

            string name = Path.GetFileNameWithoutExtension(cocoFileName)?.PadRight(8, ' ');
            string ext = Path.GetExtension(cocoFileName);

            ext = string.IsNullOrEmpty(ext) ? "   " : ext.Remove(0, 1).PadRight(3, ' ');

            if (name?.Length > 8)
            {
                throw new Exceptions.FileNameTooLongException("The coco file name must be 8 characters or less");
            }

            if (_fileNumber == 0)
            {
                for (int i = 0; i < _virtualDisk.Length; i++)
                {
                    _virtualDisk[i] = 0xff;
                }

                for (int i = gatTrackPosition; i < gatTrackPosition + 0x100; i++)
                {
                    _gatTrack[i] = 0;
                }

                for (int i = gatTrackPosition; i < gatTrackPosition + granulesPerDisk; i++)
                {
                    _gatTrack[i] = 0xff;
                }
            }

            firstGranule = _nextGranule;
            int bytesRead = 0;
            using (var reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                while (_nextGranule < gatByteOrder.Length)
                {
                    _currentGranule = _nextGranule;
                    byte[] data = reader.ReadBytes(0x900);
                    bytesRead = data.Length;
                    if (data.Length == 0) break;
                    Array.Copy(data, 0, _virtualDisk, gatByteOrder[_nextGranule] * 0x900, data.Length);
                    _nextGranule++;
                    _gatTrack[0x100 + gatByteOrder[_currentGranule]] = (byte)(data.Length == 0x900 ? gatByteOrder[_nextGranule] : 0xc1 + data.Length / 0x100);
                    if (bytesRead < 0x900) break;
                }
            }

            for (int i = gatDirectoryTrackPosition + _fileNumber * 0x20; i < gatDirectoryTrackPosition + _fileNumber * 0x20 + 0x20; i++)
            {
                _gatTrack[i] = 0;
            }

            for (int i = 0; i < 11; i++)
            {
                _gatTrack[gatDirectoryTrackPosition + _fileNumber * 0x20 + i] = Encoding.ASCII.GetBytes(name?.ToUpper() + ext.ToUpper())[i];
            }

            _gatTrack[gatDirectoryTrackPosition + _fileNumber * 0x20 + 0x0b] = (byte)fileType;
            _gatTrack[gatDirectoryTrackPosition + _fileNumber * 0x20 + 0x0c] = (byte)fileMode;
            _gatTrack[gatDirectoryTrackPosition + _fileNumber * 0x20 + 0x0d] = gatByteOrder[firstGranule];
            _gatTrack[gatDirectoryTrackPosition + _fileNumber * 0x20 + 0x0e] = (byte)(bytesRead != 0 && bytesRead % 0x100 == 0 ? 0x01 : 0x00);
            _gatTrack[gatDirectoryTrackPosition + _fileNumber * 0x20 + 0x0f] = (byte)(bytesRead % 0x100);

            Array.Copy(_virtualDisk, gatStartingPosition, _virtualDisk, gatStartingPosition + 0x900 * 2, _virtualDisk.Length - gatStartingPosition - 0x900 * 2);
            Array.Copy(_gatTrack, 0, _virtualDisk, gatStartingPosition, _gatTrack.Length);

            _fileNumber++;
        }

        public void WriteVirtualDiskToFile(string fileName, bool overwrite = true)
        {
            FileMode fm = overwrite ? FileMode.Create : FileMode.CreateNew;

            using (var bw = new BinaryWriter(File.Open(fileName, fm)))
            {
                bw.Write(_virtualDisk);
            }
        }
    }
}
