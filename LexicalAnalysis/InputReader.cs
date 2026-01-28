using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis
{
    public class InputReader
    {
        // End of file and sentinels both use the same special marker.
        // Special marker should not be in the alphabet.
        public const char SpecialMarker = '#';

        public char CurrentChar { get => _currentChar; }

        private readonly int _bufferLength;
        private readonly string _filePath;

        private int _beginPointer;
        private int _forwardPointer;
        private readonly char[] _previousBuffer;
        private readonly char[] _currentBuffer;
        private char _currentChar;
        private readonly FileStream _fileStream;
        private readonly StreamReader _streamReader;

        public InputReader(int bufferLength, string filePath)
        {
            _bufferLength = bufferLength;
            _filePath = filePath;

            _beginPointer = 2 * _bufferLength;
            // Minus 2 because the first call to AdvanceForwardPointer will advance twice.
            _forwardPointer = 2 * _bufferLength - 2;
            _currentChar = '\0';

            _previousBuffer = new char[_bufferLength];
            _currentBuffer = new char[_bufferLength];

            _fileStream = new(_filePath, FileMode.Open, FileAccess.Read);
            _streamReader = new(_fileStream, Encoding.UTF8);
        }

        public void CloseFile()
        {
            _streamReader.Close();
            _fileStream.Close();
        }

        public void AdvanceForwardPointer()
        {
            if (_forwardPointer + 1 == _bufferLength - 1)
            {
                _forwardPointer++;
            }
            _forwardPointer++;

            if (_forwardPointer == (2 * _bufferLength) - 1)
            {
                LoadNextBuffer();
            }
            UpdateCurrentChar();
        }

        public void RetractForwardPointer(int retract)
        {
            if (retract > _forwardPointer - _beginPointer) throw new Exception("Forward pointer should always be ahead or at same position of begin pointer.");

            if (_forwardPointer >= _bufferLength && _forwardPointer - retract < _bufferLength)
            {
                _forwardPointer--;
            }
            _forwardPointer -= retract;
            UpdateCurrentChar();
        }

        public string ConsumeLexeme()
        {
            string lexeme;
            if (_beginPointer < _bufferLength)
            {
                if (_forwardPointer < _bufferLength)
                {
                    int end = _forwardPointer + 1;
                    lexeme = new(_previousBuffer[_beginPointer..end]);
                }
                else
                {
                    int previousEnd = _bufferLength - 1;
                    string previousPart = new(_previousBuffer[_beginPointer..previousEnd]);
                    int currentEnd = _forwardPointer - _bufferLength + 1;
                    string currentPart = new(_currentBuffer[..currentEnd]);
                    lexeme = previousPart + currentPart;
                }
            }
            else
            {
                int start = _beginPointer - _bufferLength;
                int end = _forwardPointer - _bufferLength + 1;
                lexeme = new(_currentBuffer[start..end]);
            }
            _beginPointer = _forwardPointer + 1;
            if (_beginPointer == _bufferLength - 1)
            {
                _beginPointer++;
            }
            return lexeme;
        }

        private void UpdateCurrentChar()
        {
            if (_forwardPointer < _bufferLength)
            {
                _currentChar = _previousBuffer[_forwardPointer];
            }
            else
            {
                _currentChar = _currentBuffer[_forwardPointer - _bufferLength];
            }
        }

        private void LoadNextBuffer()
        {
            // Buffer length is not sufficient for the current lexeme size.
            if (_beginPointer < _bufferLength) throw new Exception("Begin is pointing at previous buffer, loading next buffer will corrupt current lexeme.");

            _currentBuffer.CopyTo(_previousBuffer, 0);
            Array.Clear(_currentBuffer);
            _beginPointer -= _bufferLength;
            _forwardPointer = _bufferLength;

            int charsRead = _streamReader.Read(_currentBuffer, 0, _bufferLength - 1);

            /* ------- À COMPLÉTER ------- */
            if (charsRead < _bufferLength - 1)
            {
                _currentBuffer[charsRead] = SpecialMarker;
            } else
            {
                _currentBuffer[_bufferLength - 1] = SpecialMarker;
            }
        }
    }
}
