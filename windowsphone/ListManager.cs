using System;
using System.Collections.Generic;
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
                    base.AddRange(fill);
                    Save();
                }
                else
                {
                    foreach (DictionaryRecord r in new Dictionary(Location))
                        base.Add(r);
                }
            }

            string Location;
            public string Name
            {
                get
                {
                    return Path.GetFileNameWithoutExtension(Location);
                }
            }

            public new void Add(DictionaryRecord item)
            {
                base.Add(item);
                Save();
            }

            public new void Remove(DictionaryRecord item)
            {
                base.Remove(item);
                Save();
            }

            void Save()
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
        }

        public ListManager()
        {
            
        }

        public static List CreateList(string name, List<DictionaryRecord> content)
        {
            return new List(String.Format("{0}/{1}.list", ListsDirectory, name), content);
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
                            _Lists.Add(new List(String.Format("{0}/{1}", ListsDirectory, file)));
                }
                catch (Exception)
                {
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
