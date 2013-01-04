using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

using CC_CEDICT.WindowsPhone;

namespace kuaishuo2
{
    class ListManager
    {
        public class List : List<DictionaryRecord>
        {
            public List(string path, List<DictionaryRecord> fill = null)
            {
                this.Location = path;

                if (fill != null)
                {
                    this.AddRange(fill);
                    Save();
                }
                else
                {
                    Dictionary d = new Dictionary(Location);
                    foreach (DictionaryRecord r in d)
                        this.Add(r);
                }
            }

            public string Location { get; private set; }
            public string Name
            {
                get
                {
                    return Path.GetFileNameWithoutExtension(Location);
                }
            }

            void Save()
            {
                try
                {
                    using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (store.FileExists(Location))
                            store.DeleteFile(Location);
                        IsolatedStorageFileStream stream = store.CreateFile(Location);
                        foreach (DictionaryRecord r in this)
                        {
                            string line = String.Format("{0}\n", r.ToString());
                            byte[] data = Encoding.UTF8.GetBytes(line.ToCharArray());
                            stream.Write(data, 0, data.Length);
                        }
                        stream.Close();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to Save() List: {0}", ex.Message);
                }
            }
        }

        public ListManager()
        {
            
        }

        public static List CreateList(string name, List<DictionaryRecord> content)
        {
            return new List(String.Format("{0}/{1}.list", ListsDirectory, name), content);
        }

        public static List GetListByName(string name)
        {
            return new List(String.Format("{0}/{1}.list", ListsDirectory, name));
        }

        List<List> _Lists;
        public List<List> Lists
        {
            get
            {
                _Lists = new List<List>();
                try
                {
                    using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                        foreach (string file in store.GetFileNames(String.Format("{0}/*.list", ListsDirectory)))
                        {
                            List temp = new List(String.Format("{0}/{1}", ListsDirectory, file));
                            _Lists.Add(temp);
                            Debug.WriteLine("_Lists now contains {0} items", _Lists.Count);
                        }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Problem loading Lists: {0}", ex.Message);
                }
                return _Lists;
            }
        }

        const string _ListsDirectory = "lists";
        static string ListsDirectory
        {
            get
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                    if (!store.DirectoryExists(_ListsDirectory))
                        store.CreateDirectory(_ListsDirectory);
                return _ListsDirectory;
            }
        }
    }
}
