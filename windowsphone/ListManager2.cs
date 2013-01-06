using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

using CC_CEDICT.WindowsPhone;

namespace kuaishuo2
{
    public class ListManager2 : Dictionary<string, DictionaryRecordList>, IDisposable
    {
        public ListManager2()
        {
            this.Reload();
        }

        /// <summary>
        /// Contains the file numbers of the *.list files, indexed by name.
        /// </summary>
        Dictionary<string, int> ListNumberLookup = new Dictionary<string, int>();

        public void Reload()
        {
            this.Clear();

            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    foreach (string file in store.GetFileNames(String.Format("{0}/*.list", ListsDirectory)))
                    {
                        string path = String.Format("{0}/{1}", ListsDirectory, file);
                        Dictionary dictionary = new Dictionary(path);
                        DictionaryRecordList list = new DictionaryRecordList(dictionary);
                        list.SavePath = path;
                        this.Add(list.Name, list);
                        ListNumberLookup.Add(list.Name, int.Parse(Path.GetFileNameWithoutExtension(path)));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problem loading lists: {0}", ex.Message);
            }
        }

        public new DictionaryRecordList this[string name]
        {
            get
            {
                if (!this.ContainsKey(name))
                {
                    DictionaryRecordList list = new DictionaryRecordList(name);
                    this.Add(name, list);
                    int x = -1;
                    foreach (int n in ListNumberLookup.Values)
                        if (n > x)
                            x = n;
                    ListNumberLookup.Add(name, ++x);
                    list.SavePath = ListFilePath(name);
                }
                return base[name];
            }
        }

        public string ListFilePath(string name)
        {
            return String.Format("{0}/{1}.list", ListsDirectory, ListNumberLookup[name]);
        }

        static bool DirectoryExists = false;
        const string _ListsDirectory = "lists";
        static string ListsDirectory
        {
            get
            {
                if (!DirectoryExists)
                {
                    using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                        if (!store.DirectoryExists(_ListsDirectory))
                            store.CreateDirectory(_ListsDirectory);
                    DirectoryExists = true;
                }
                return _ListsDirectory;
            }
        }

        bool Disposed = false;
        public void Dispose()
        {
            if (Disposed) // already disposed
                return;
            foreach (DictionaryRecordList list in this.Values)
                list.Save();
            Disposed = true;
        }
    }
}
