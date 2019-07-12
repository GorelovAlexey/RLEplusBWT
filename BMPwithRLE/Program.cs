using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading.Tasks;

namespace RLEplusBWT
{
    class  MergeSortGeneric
    {
        public static void Sort<T> (IList<T> data, IComparer<T> comparer, int left, int right) 
        {
            int len = right - left;
            // сортировка слиянием начиная с arr[left] и ДО arr[right] 
            if (len > 1)
            {
                // long чтобы избежать переполнения к примеру для случая когда int.MAX и int.MAX - 1;
                var mid = (int)((long)right + left) / 2;

                // Разделяй и властвуй
                if (len >= 2048)
                {
                    var t1 = Task.Run(() => Sort(data, comparer, left, mid));
                    var t2 = Task.Run(() => Sort(data, comparer, mid, right));
                    Task.WaitAll(t1, t2);
                }
                else
                {
                    Sort(data, comparer, left, mid);
                    Sort(data, comparer, mid, right);
                }

                // Слияние отсортированных подмассивов [left, mid) [mid, right)
                Merge(data, comparer, left, mid, right);
            }
        }
        private static void Merge<T>(IList<T> data, IComparer<T> comparer, int left, int middle, int right)
        {
            // Инициализация временных массиовов
            var L = new T[middle - left];
            var R = new T[right - middle];

            for (int i = 0; i < L.Length; i++)
                L[i] = data[left + i];
            for (int i = 0; i < R.Length; i++)
                R[i] = data[middle + i];

            // Слияние отсортированных массивов и запись в исходный
            int iLeft = 0;
            int iRight = 0;
            int j = left;

            while (iLeft < L.Length && iRight < R.Length)
            {
                if (comparer.Compare(L[iLeft], R[iRight]) <= 0)
                {
                    data[j++] = L[iLeft++];
                }
                else
                {
                    data[j++] = R[iRight++];
                }
            }

            while (iLeft < L.Length)
            {
                data[j++] = L[iLeft++];
            }

            while (iRight < R.Length)
            {
                data[j++] = R[iRight++];
            }
        }
    }
    class OffsetArrayComparer : Comparer<OffsetArray>
    {
        public override int Compare(OffsetArray x, OffsetArray y)
        {
            for (int i = 0; i < x.Count; i++)
            {
                if (x[i] != y[i])
                {
                    if (x[i] > y[i]) return 1;
                    else return -1;
                }
            }
            return 0;
        }
    }
    class ValueTupleComparer : IComparer<ValueTuple<byte, int>>
    {
        public int Compare((byte, int) x, (byte, int) y)
        {
            if (x.Item1 > y.Item1) return 1;
            else if (x.Item1 == y.Item1) return -1;
            else return 0;
        }
    }

    // Перестановка исходного подмассива, где начальный элемент сдвигается на offset элементов
    // Реализован не полностью, используется для того чтобы задействовать стандартную сортировку 
    class OffsetArray : IList<byte>, IComparable<OffsetArray>
    {
        public int Offset { get; }
        public byte[] Array { get; }


        public OffsetArray(byte[] array, int offset)
        {
            this.Array = array;
            this.Offset = offset;
        }
        
        public byte this[int index]
        {
            get
            {
                if (index < 0 || index >= Array.Length) throw new IndexOutOfRangeException();
                return this.Array[(index + Offset) % Count];
            }
            set
            {
                if (index < 0 || index >= Array.Length) throw new IndexOutOfRangeException();
                this[(index + Offset) % Count] = value;
            }
        }

        public int Count => Array.Length;

        public bool IsReadOnly => throw new NotImplementedException();

        public int IndexOf(byte item)
        {
            for (int i = 0; i < this.Count; i++)
                if (this[i] == item) return i;
            return -1;
        }

        public void Insert(int index, byte item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(byte item)
        {
            foreach (var b in Array) if (b == item) return true;
            return false;
        }
        
        public void CopyTo(byte[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        
        public void Add(byte item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(byte item)
        {
            throw new NotImplementedException();
        }

        
        // Используется для сортиорвки стандартными 
        public int CompareTo(OffsetArray other)
        {
            if (other.Count != this.Count) return CompareToUnequal(other);
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i] != other[i])
                {
                    if (this[i] > other[i]) return 1;
                    else return -1;
                }
            }
            return 0;
        }
        int CompareToUnequal(OffsetArray other)
        {
            int lowest = (Count > other.Count) ? other.Count : Count;
            for (int i = 0; i < lowest; i++)
            {
                if (this[i] != other[i])
                {
                    if (this[i] > other[i]) return 1;
                    else return -1;
                }
            }
            if (lowest < Count) return 1;
            else if (lowest > Count) return -1;
            else return 0;
        }
        public IEnumerator<byte> GetEnumerator()
        {
            return new OffsetArrayEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new OffsetArrayEnumerator(this);
        }

        class OffsetArrayEnumerator :  IEnumerator<byte>
        {
            public OffsetArray _array;
            int position = -1;

            public OffsetArrayEnumerator(OffsetArray a)
            {
                _array = a;
            }

            public byte Current
            {
                get { return _array[position]; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
                // Потенциальная? утечка? памяти?
            }

            public bool MoveNext()
            {
                ++position;
                return (position < _array.Count);
            }

            public void Reset()
            {
                position = -1;
            }
        }
    }

    // Класс совершающий RLE кодирование/декодирование
    class RLE
    {
        const byte SPECAL = 127; // Спецсимвол которым кодируется последовательность одинаковых байтов
        const int MIN_LENGTH_TO_SUBSTITUTE = 4; // Минимальное количество повторяющихся байтов которые заменяются RL-последовательностью

        // Поиск в массиве байтов самого редкого байта
        private static byte LowestFreq(byte[] a)
        {
            int[] freq = new int[256];
            foreach (var b in a) freq[b] += 1;
            byte min = 0;
            for (int i = 1; i < freq.Length; i++) if (freq[i] < freq[min]) min = (byte)i;
            return min;
        }

        // RLE кодирование массива байтов
        public static List<byte> Encode(byte[] a, bool useFlexibleSpecial)
        {
            byte specialByte = SPECAL;
            if (useFlexibleSpecial) specialByte = LowestFreq(a);

            var result = new List<byte> { specialByte };
            var arrayLen = a.Length;

            for (int pos = 0; pos < arrayLen; pos++)
            {
                var repeatingLength = 1;
                var currentByte = a[pos];

                // проверяем повторяется ли текущий байт далее и если да то сколько раз
                while (pos + 1 < arrayLen 
                    && a[pos + 1] == currentByte 
                    && repeatingLength < byte.MaxValue)
                {
                    pos++;
                    repeatingLength++;
                }

                if (repeatingLength > byte.MaxValue) throw new Exception("Repeating length > byte max value");

                // Повторяющаяся последовательность или спец символ заменяются на 
                // СПЕЦСИВОЛ + длинна (1-255) + ПООВТОРЯЮЩИЙСЯ СИМВОЛ
                // Иначе символы добавляются без изменений
                if (currentByte == specialByte || repeatingLength >= MIN_LENGTH_TO_SUBSTITUTE) 
                {
                    result.Add(specialByte);
                    result.Add((byte)repeatingLength);
                    result.Add(currentByte);
                }
                else for (int i = 0; i < repeatingLength; i++) result.Add(currentByte);      
            }            
            return result;
        }       

        // RLE декодирование массива байтов
        public static byte[] Decode(byte[] a)
        {
            var result = new List<byte>();
            var specialByte = a[0];

            int pos = 1;

            while (pos < a.Length)
            {
                if (a[pos] == specialByte)
                {
                    var len = a[pos + 1];
                    var symb = a[pos + 2];
                    for (byte i = 0; i < len; i++) result.Add(symb);
                    pos += 2;
                }
                else result.Add(a[pos]);
                pos++;
            }
            return result.ToArray();
        }
    }

    // Кодирование BWT и настройки
    class BWT
    {
        const ushort BLOCK_SIZE = 512;
        const int OFFSET_SIZE = sizeof(ushort);

      // обратная трансформация BWT, рзбивается на блоки
        public static byte[] Decode(byte[] input)
        {
            int blockSize = BLOCK_SIZE + OFFSET_SIZE;

            int start = 0;
            int end = blockSize;
            var blocks = new List<byte[]>();

            while (start < input.Length)
            {
                if (end > input.Length) end = input.Length;
                var blockLength = end - start;
                var data = new byte[blockLength];
                Array.Copy(input, start, data, 0, blockLength);
                blocks.Add(DecodeBlock(data));
                start = end;
                end += blockSize;
            }

            int totalSize = 0;
            foreach (var b in blocks) totalSize += b.Length;
            var result = new byte[totalSize];
            start = 0;
            foreach (var b in blocks)
            {
                Array.Copy(b, 0, result, start, b.Length);
                start += b.Length;
            }
            return result;
        }
        
        static byte[] DecodeBlock(byte[] input)
        {
            ushort originalIndex = BitConverter.ToUInt16(input, 0);

            var arr = TakeDataAndPairWithIndex(input);

            MergeSortGeneric.Sort(arr, new ValueTupleComparer(), 0, arr.Length);
            
            return GetOriginalBlock(arr, originalIndex);
        }

        static (byte, int)[] TakeDataAndPairWithIndex(byte[] input)
        {
            var ret = new (byte, int)[input.Length - OFFSET_SIZE];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (input[i + OFFSET_SIZE], i);
            }
            return ret;
        }

        static byte[] GetOriginalBlock((byte value, int index)[] arr, int index)
        {
            var data = new byte[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                data[i] = arr[index].value;
                index = (ushort)arr[index].index;
            }
            return data;
        }


        public static byte[] Encode(byte[] input)
        {
            int start = 0;
            int end = BLOCK_SIZE;
            var blocks = new List<byte[]>();
            while (start < input.Length)
            {
                if (end > input.Length) end = input.Length;
                var blockLength = end - start;
                var data = new byte[blockLength];
                Array.Copy(input, start, data, 0, blockLength);
                blocks.Add(EncodeBlock(data, (ushort)blockLength));
                start = end;
                end += BLOCK_SIZE;
            }
            int totalSize = 0;
            foreach (var b in blocks) totalSize += b.Length;
            var result = new byte[totalSize];
            start = 0;
            foreach (var b in blocks)
            {
                Array.Copy(b, 0, result, start, b.Length);
                start += b.Length;
            }
            return result;
        }

        static byte[] EncodeBlock(byte[] input, ushort size)
        {
            if (input.Length > ushort.MaxValue) throw new ArgumentOutOfRangeException("encode block too big");

            ushort BLOCK_SIZE = size;
            byte[] result = new byte[BLOCK_SIZE + OFFSET_SIZE];

            var offsetArrays = new List<OffsetArray>();
            for (int i = 0; i < input.Length; i++) offsetArrays.Add(new OffsetArray(input, i));

            offsetArrays.Sort();

            ushort originalPosition = 0;
            for (ushort i = 0; i < offsetArrays.Count; i++)
            {
                if (offsetArrays[i].Offset == 0)
                {
                    originalPosition = i;
                    break;
                }
            }

            Array.Copy(BitConverter.GetBytes(originalPosition), result, OFFSET_SIZE);

            for (int i = 0; i < offsetArrays.Count; i++)
            {
                result[i + OFFSET_SIZE] = offsetArrays[i].Last();
            }

            return result;
        }

    }

    // Клаcc для работы с файлами
    class FileWorker
    {
        public void FileEncoder (string target, bool useBwt = true, bool useFlexSpecial = false)
        {
            var file = LoadFile(target);
            var result = Encode(file, useBwt, useFlexSpecial);
            var saveFile = GetFileNameEncoded(target);
            File.WriteAllBytes(saveFile, result);
        }

        public void FileDecoder (string target, bool useBwt = true)
        {
            var file = LoadFile(target);
            var result = Decode(file, useBwt);
            var saveFile = GetFileNameDecoded(target);

            File.WriteAllBytes(saveFile, result);
        }

        public byte[] LoadFile (string file)
        {
            return File.ReadAllBytes(file);
        }
 
        public string GetFileNameEncoded(string original)
        {
            return original + ".rle";
        }

        public string GetFileNameDecoded(string encoded)
        {
            return encoded.Remove(encoded.Length - 4, 4);
        }

        byte[] Encode(byte[] file, bool useBwt = true, bool useFlexSpecial = false)
        {
            int originalSize = file.Length;


            var result = RLE.Encode(file, useFlexSpecial).ToArray();
            if (useBwt)
            {
                result = BWT.Encode(result);
                result = RLE.Encode(result, useFlexSpecial).ToArray();
            }

            int resultSize = result.Length ;
            double compression = 100 - (double)resultSize * 100 / originalSize;

            Console.WriteLine($" Размер оригинала: {originalSize} результат: {resultSize} Сжатие: {compression}%");
            return result;
        }

        byte[] Decode(byte[] a, bool bwt = true)
        {
            a = RLE.Decode(a);
            if (bwt)
            {
                a = RLE.Decode(BWT.Decode(a));
            }
            return a;
        }


    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;                       
            var BMP = new FileWorker();

            BMP.FileEncoder(@"Pics\CHB.bmp", useFlexSpecial: true);
            BMP.FileDecoder(@"Pics\CHB.bmp.rle");

            BMP.FileEncoder(@"Pics\CHB2.bmp", useFlexSpecial: true);
            BMP.FileDecoder(@"Pics\CHB2.bmp.rle");

            BMP.FileEncoder(@"Pics\CV.bmp", useFlexSpecial: true);
            BMP.FileDecoder(@"Pics\CV.bmp.rle");

            BMP.FileEncoder(@"Pics\CV2.bmp", useFlexSpecial: true);
            BMP.FileDecoder(@"Pics\CV2.bmp.rle");

            BMP.FileEncoder(@"Pics\Real.bmp", useFlexSpecial: true);
            BMP.FileDecoder(@"Pics\Real.bmp.rle");

            BMP.FileEncoder(@"Pics\Real2.bmp", useFlexSpecial: true);
            BMP.FileDecoder(@"Pics\Real2.bmp.rle");

            BMP.FileEncoder(@"Pics\RealCB.bmp", useFlexSpecial: true);
            BMP.FileDecoder(@"Pics\RealCB.bmp.rle");


            Console.ReadKey();
        }
    }
}
