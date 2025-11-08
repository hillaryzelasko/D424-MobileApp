using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WGUTermTracker
{
    public static class Constants
    {
        public const string DatabaseFilename = "WGUTermTrackerSQLite.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            // OPEN THE DATABASE IN R/W MODE
            SQLite.SQLiteOpenFlags.ReadWrite |
            // CREATES DB IF IT DOESN'T EXIST
            SQLite.SQLiteOpenFlags.Create |
            // ENABLES MULTI-THREADED ACCESS
            SQLite.SQLiteOpenFlags.SharedCache;


        // JOINS FOLDER PATHS FOR OS
        public static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
            
    }
}
