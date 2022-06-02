class SharableSpreadSheet
{
    private bool[][] locks;
    private String[][] spreadsheet;
    private int rows, cols, curr_searching = 0;
    private int nUsers = -2;
    private int lockAmount = 5; //TODO change accordingly.
    //TODO syncs

    
    public SharableSpreadSheet(int nRows, int nCols, int nUsers = -1)
    {
        // construct a nRows*nCols spreadsheet
        if (nRows <= 0)
            throw new ArgumentOutOfRangeException(nameof(nRows));

        if (nCols <= 0)
            throw new ArgumentOutOfRangeException(nameof(nCols));


        spreadsheet = new String[nRows][];
        locks = new bool[nRows][];

        for (int i = 0; i < nRows; i++)
        {
            spreadsheet[i] = new String[nCols];
            locks[i] = new bool[nCols];
        }
        setConcurrentSearchLimit(nUsers);
        rows = nRows;
        cols = nCols;
    }

    public String getCell(int row, int col)
    {
        // return the string at [row,col]

        if (row < 0 || row > rows)
            throw new ArgumentOutOfRangeException(nameof(row));
        if (col < 0 || col > cols)
            throw new ArgumentOutOfRangeException(nameof(col));

        return spreadsheet[row][col];
    }
    
    public void setCell(int row, int col, String str)
    {
        // set the string at [row,col]
        if (row < 0 || row > rows)
            throw new ArgumentOutOfRangeException(nameof(row));
        if (col < 0 || col > cols)
            throw new ArgumentOutOfRangeException(nameof(col));
        if (str == null)
            throw new ArgumentNullException(nameof(str));

        LockCell(row, col, true);
        spreadsheet[row][col] = str;
        LockCell(row, col, false);
    }

    public Tuple<int, int> searchString(String str)
    {
        int row, col;

        // search the cell with string str, and return true/false accordingly.
        // return first cell indexes that contains the string (search from first row to the last row)

        row = 0;
        col = 0;
        int temp1, temp2;
        while (row < rows)
        {
            temp1 = row;
            temp2 = col;
            LockSequence(row, col, true);
            for (int i = 0; i < lockAmount; i++)
            {
                if (spreadsheet[row][col] == str)
                {
                    LockSequence(temp1, temp2, false);
                    return new Tuple<int, int>(row, col);
                }

                col++;
                if (col >= cols)
                {
                    col = 0;
                    row++;
                    if (row >= rows)
                        break;
                }
            }
            LockSequence(temp1, temp2, false);

        }
        throw new Exception($"String \"{str}\" not found.");
    }
    
    public void exchangeRows(int row1, int row2) //TODO exchanges via locks more efficiantly.
    {
        // exchange the content of row1 and row2

        if (row1 < 0 || row1 > rows)
            throw new ArgumentOutOfRangeException(nameof(row1));
        if (row2 < 0 || row2 > rows)
            throw new ArgumentOutOfRangeException(nameof(row2));

        if (row1 == row2)
            return;

        string temp;
        int i = 0;
        while (i < cols)
        {
            int _ = i;
            LockCol(row1, i, true);
            LockCol(row2, i, true);
            while (i < cols)
            {
                temp = spreadsheet[row1][i];
                spreadsheet[row1][i] = spreadsheet[row2][i];
                spreadsheet[row2][i] = temp;
                i++;
            }
            LockCol(row1, _, false);
            LockCol(row2, _, false);
        }
    }
    
    public void exchangeCols(int col1, int col2)
    {
        // exchange the content of col1 and col2

        if (col1 < 0 || col1 > rows)
            throw new ArgumentOutOfRangeException(nameof(col1));
        if (col2 < 0 || col2 > rows)
            throw new ArgumentOutOfRangeException(nameof(col2));

        if (col1 == col2)
            return;

        string temp;
        int i = 0;
        while (i < rows)
        {
            int _ = i;
            LockRow(i, col1, true);
            LockRow(i, col2, true);
            while (i < rows)
            {
                temp = spreadsheet[i][col1];
                spreadsheet[i][col1] = spreadsheet[i][col2];
                spreadsheet[i][col2] = temp;
                i++;
            }
            LockRow(_, col1, false);
            LockRow(_, col2, false);
        }
    }

    // ******************************************************************************************************************* Done until this line.
    // TODO: base all multi-cell searches on SearchInRange.
    public int searchInRow(int row, String str)
    {
        int col;
        // perform search in specific row

        if (row < 0 || row > rows)
            throw new ArgumentOutOfRangeException(nameof(row));
        LockSearch();

        for (col = 0; col < rows; col++)
            if (spreadsheet[row][col] == str)
            {
                UnlockSearch();
                return col;
            }

        UnlockSearch();
        throw new Exception($"String {str} not found in row {row}.");
    }
    
    public int searchInCol(int col, String str)
    {
        LockSearch();

        int row;
        // perform search in specific col

        if (col < 0 || col > cols)
            throw new ArgumentOutOfRangeException(nameof(col));

        for (row = 0; row < rows; row++)
            if (spreadsheet[row][col] == str)
                if (spreadsheet[row][col] == str)
                {
                    UnlockSearch();
                    return row;
                }
        throw new Exception($"String {str} not found in col {col}.");
    }
    
    public Tuple<int, int> searchInRange(int col1, int col2, int row1, int row2, String str)
    {
        LockSearch();
        int row, col;
        // perform search within spesific range: [row1:row2,col1:col2] 
        //includes col1,col2,row1,row2
        if (col1 > col2 || row1 > row2)
            throw new ArgumentException("Range start can't be bigger than its end.");
        if (str == null)
            throw new ArgumentNullException(nameof(str));
        for (row = row1; row <= row2; row++)
            for (col = col1; col <= col2; col++)
                if (spreadsheet[row][col] == str)
                {
                    UnlockSearch();
                    return new Tuple<int, int>(row, col);
                }
        throw new Exception($"String {str} not found in range [{col1}:{col2},{row1}:{row2}].");
    }
    
    public void addRow(int row1)
    {
        //add a row after row1
        if (row1 < 0 || row1 > rows - 1)
            throw new ArgumentOutOfRangeException(nameof(row1));
        string[][] newsheet = new string[rows + 1][];
        int i;
        for (i = 0; i < row1; i++)
            newsheet[i] = spreadsheet[i];
        _ = spreadsheet[row1];
        newsheet[row1] = new string[cols];

        rows++;
        for (i = row1 + 1; i < rows; i++)
        {
            newsheet[i] = spreadsheet[i - 1];
        }

        spreadsheet = newsheet;
    }
    
    public void addCol(int col1)
    {
        //add a column after col1
        if (col1 < 0 || col1 > cols - 1)
            throw new ArgumentOutOfRangeException(nameof(col1));

        string[][] newsheet = new string[rows][];
        cols++;
        for (int i = 0; i < rows; i++)
        {
            newsheet[i] = new string[cols];
            string[] curr_row = spreadsheet[i];
            int j;
            for (j = 0; j < col1; j++)
            {
                newsheet[i][j] = curr_row[j];
            }
            for (j = col1 + 1; j < cols; j++)
            {
                newsheet[i][j] = spreadsheet[i][j - 1];
            }
        }
        spreadsheet = newsheet;
    }
    
    public Tuple<int, int>[] findAll(String str, bool caseSensitive)
    {
        // perform search and return all relevant cells according to caseSensitive param
        LockSearch();
        List<Tuple<int, int>> list = new();
        int row, col;
        for (row = 0; row < rows; row++)
            for (col = 0; col < spreadsheet[row].Length; col++)
                if (caseSensitive)
                {
                    if (string.Equals(spreadsheet[row][col], str))
                        list.Add(new Tuple<int, int>(row, col));
                }
                else
                    if (string.Equals(spreadsheet[row][col], str, StringComparison.OrdinalIgnoreCase))
                    list.Add(new Tuple<int, int>(row, col));


        UnlockSearch();
        return list.ToArray();
    }
    
    public void setAll(String oldStr, String newStr, bool caseSensitive)
    {
        // replace all oldStr cells with the newStr str according to caseSensitive param
        curr_searching--;
        Tuple<int, int>[] replaces = findAll(oldStr, caseSensitive);
        curr_searching++;
        for (int i = 0; i < replaces.Length; i++)
            setCell(replaces[i].Item1, replaces[i].Item2, newStr);
    }
    
    public Tuple<int, int> getSize()
    {
        // return the size of the spreadsheet in nRows, nCols
        return new Tuple<int, int>(rows, cols);
    }
    
    public void setConcurrentSearchLimit(int nUsers)
    {
        // this function aims to limit the number of users that can perform the search operations concurrently.
        // The default is no limit. When the function is called, the max number of concurrent search operations is set to nUsers. 
        // In this case additional search operations will wait for existing search to finish.
        // This function is used just in the creation
        if (nUsers != -2)
            return;
        if (nUsers < -1)
            throw new ArgumentOutOfRangeException(nameof(nUsers));
        this.nUsers = nUsers;
    }
    
    public void save(String fileName)
    {
        // save the spreadsheet to a file fileName.
        // you can decide the format you save the data. There are several options.
    }
    
    public void load(String fileName)
    {
        // load the spreadsheet from fileName
        // replace the data and size of the current spreadsheet with the loaded data
    }

    private void LockSearch()
    {
        if (nUsers == -1)
            return;

        while (curr_searching >= nUsers) ;

        curr_searching++;
    }

    private void UnlockSearch()
    {
        if (nUsers == -1)
            return;
        curr_searching--;
    }

    private void LockCells(int row1, int row2, int col1, int col2, bool Lock)
    {
        int row, col;
        for (row = row1; row <= row2; row++)
            for (col = col1; col <= col2; col++)
                LockCell(row, col, Lock);
    }

    private void LockRow(int row, int col, bool Lock)
    {
        int temp = row + lockAmount;
        while (row < rows || row < temp)
            LockCell(row++, col, Lock);
    }

    private void LockCol(int row, int col, bool Lock)
    {
        int temp = col + lockAmount;
        while (col < cols || col < temp)
            LockCell(row, col++, Lock);
    }

    private void LockCell(int row, int col, bool Lock)
    {
        if (row >= rows || col >= cols)
            return;
        if (Lock)
            while (locks[row][col]) ;
        locks[row][col] = Lock;
    }
    
    private void LockSequence(int row, int col, bool Lock)
    {
        for (int i = 0; i < lockAmount; i++)
        {
            LockCell(row, col++, Lock);
            if (col >= cols)
            {
                row++;
                col = 0;
                if (row >= rows)
                    return;
            }
        }
    }




    //TODO: remove. Code below used for debugging.
    static void Main()
    {
        SharableSpreadSheet sheet = new(5, 5);
        for (int i = 0; i < sheet.rows; i++)
            for (int j = 0; j < sheet.cols; j++)
                sheet.setCell(i, j, (i * 5 + j).ToString());
        sheet.print();
        sheet.exchangeCols(0, 2);

        sheet.printStatus();
    }

    
    public void print()
    {
        Console.WriteLine("Spreadsheet:");
        foreach (string[] row in spreadsheet)
        {
            foreach (string s in row)
                Console.Write($"[{s}]");
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    public void printLocks()
    {
        string _;
        Console.WriteLine("Locks:");

        foreach (bool[] row in locks)
        {
            foreach (bool s in row)
            {
                _ = s ? "T" : "F";
                Console.Write($"[{_}]");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    public void printStatus()
    {
        print();
        printLocks();
    }


}