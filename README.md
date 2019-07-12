  Класс FileWorker сжимает файлы используя следующие методы RLE -> BWT -> RLE 
  Результат записывает в той же папке с исходным файлом добавляя расширение .rle
  Обратная операция выполняет обратное преобразование и сохраняет файл с тем же именем но без расширения .rle

  Классы RLE и BWT отвечают за соответствующие преобразования.

  BWT выполняется по блокам. Размер блока определяется в классе BWT как ushort BLOCK_SIZE.
  Ushort - целое число без знака 16 бит. Чем больше размер блока тем лучше получается сжатие, но увеличивается время работы программы. 
  В начале каждого блока записываются 2 байта с данными для обратного преобразования, после чего идут данные. 

  В BWT используется специальный класс OffsetArray - он частично реализует интерфейсы IList и IComparible (части нужные для сортировоки). Этот класс представляет собой массив с цикличным смещением вправо. В нем хранится смещение и ссылка на исходный массив. 
  Сортировка осуществляется с помощью параллельной сортировки слиянием. Если число сортируемых элементов больше 1024 то они сорируются параллельно (с использованием Task'ов) иначе они сортируются последовательно. 
  Сортировка выполнена с помощью любого типа и Comparer'a для него или с помощью типа который реализует интерфейс IComparible.

  RLE записывает перед данными спец символ - символ которым обозначается последовательность повторяющихся байтов. Изначально это число 127, но в параметрах кодирубщего метода можно передать flexSpecial=true, тогда кодирующим сиволом будет выбран символ который встречается реже всего. 

Тесты для Real2.bmp
Размер_блока_BWT | Сжатие | Время
512              | 26%    |  6.54
65 535‬           | 32%    | 23.482
