Windows sqlite3.dll binary from here:
https://www.sqlite.org/2019/sqlite-dll-win32-x86-3280000.zip

MacOS libsqlite3.0.dylib from azure pipeline here:
https://dev.azure.com/Lidarr/Lidarr/_build?definitionId=4&_a=summary

System.Data.SQLite netstandard2.0 dll compiled in same pipeline with:
/p:Configuration=ReleaseManagedOnly /p:UseInteropDll=false /p:UseSqliteStandard=true

Both MacOS and System.Data.SQLite from revision 40e714a of https://github.com/lidarr/SQLite.Build
