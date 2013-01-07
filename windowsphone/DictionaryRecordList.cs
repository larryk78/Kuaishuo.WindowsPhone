using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Text;

using CC_CEDICT.WindowsPhone;

namespace kuaishuo2
{
    public class DictionaryRecordList : List<DictionaryRecord>
    {
        public Dictionary Dictionary;

        /// <summary>
        /// Constructor for existing lists.
        /// </summary>
        /// <param name="dictionary">CC_CEDict.WindowsPhone instance containing the entries for the list.</param>
        public DictionaryRecordList(Dictionary dictionary)
        {
            this.Dictionary = dictionary;
        }

        /// <summary>
        /// Ensure all Dictionary data has been loaded.
        /// </summary>
        /// <remarks>Provided as a separate method for manual lazy loading.</remarks>
        public void Slurp()
        {
            if (Dictionary == null)
                return;

            foreach (DictionaryRecord record in this.Dictionary)
                base.Add(record);
        }

        /// <summary>
        /// Indicates whether the list dictionary file needs to be (re-)written to disk.
        /// </summary>
        bool SaveRequired = false;

        /// <summary>
        /// Constructor for new lists.
        /// </summary>
        /// <param name="name">The name of the list to be created.</param>
        public DictionaryRecordList(string name)
        {
            this.Name = name;
            SaveRequired = true;
        }

        const string NameHeaderKey = "name";
        string _Name;
        public string Name
        {
            get
            {
                if (_Name == null)
                    _Name = this.Dictionary.Header[NameHeaderKey];
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        const string ReadOnlyHeaderKey = "readonly";
        bool ReadOnlyHeaderProcessed = false;
        bool _ReadOnly = false;
        public bool ReadOnly
        {
            get
            {
                if (!ReadOnlyHeaderProcessed)
                {
                    if (this.Dictionary != null && this.Dictionary.Header.ContainsKey(ReadOnlyHeaderKey))
                    {
                        try
                        {
                            _ReadOnly = bool.Parse(this.Dictionary.Header[ReadOnlyHeaderKey]);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    ReadOnlyHeaderProcessed = true; // whether it worked or not
                }
                return _ReadOnly;
            }
        }

        public new int Count
        {
            get
            {
                return (base.Count == 0 && this.Dictionary != null) ? this.Dictionary.Count : base.Count;
            }
        }

        public new DictionaryRecord this[int index]
        {
            get
            {
                try
                {
                    return base[index]; // see if we have a cached copy of the DictionaryRecord
                }
                catch (IndexOutOfRangeException) // nope; so load/cache/return it from Dictionary
                {
                    return base[index] = this.Dictionary[index];
                }
            }
            set
            {
                throw new NotImplementedException(); // use Add/Remove
            }
        }

        public new void Add(DictionaryRecord record)
        {
            base.Add(record);
            SaveRequired = true;
        }

        public new void Remove(DictionaryRecord record)
        {
            base.Remove(record);
            SaveRequired = true;
        }

        public string SavePath;
        public void Save()
        {
            if (!SaveRequired)
                return;

            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (this.Dictionary != null)
                    {
                        this.Dictionary.Dispose(); // closes the stream
                        this.Dictionary = null;
                    }

                    if (store.FileExists(SavePath))
                        store.DeleteFile(SavePath);

                    IsolatedStorageFileStream stream = store.CreateFile(SavePath);

                    // write the name of this list as a header
                    string header = String.Format("#! {0}={1}\n#! {2}={3}\n", NameHeaderKey, this.Name, ReadOnlyHeaderKey, this.ReadOnly);
                    byte[] headerdata = Encoding.UTF8.GetBytes(header);
                    stream.Write(headerdata, 0, headerdata.Length);

                    // write the content of this list as dictionary records
                    foreach (DictionaryRecord record in this)
                    {
                        byte[] data = Encoding.UTF8.GetBytes(String.Format("{0}\n", record.ToString()));
                        stream.Write(data, 0, data.Length);
                    }
                    
                    stream.Close();
                }
                
                SaveRequired = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to Save() List: {0}", ex.Message);
            }
        }
    }
}
