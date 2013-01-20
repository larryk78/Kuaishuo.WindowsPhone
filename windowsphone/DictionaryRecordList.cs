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
        /// <summary>
        /// Indicates whether this list has been modified since it was loaded.
        /// </summary>
        /// <remarks>
        /// Effectively indicates whether the ListManager needs to save it to disk.
        /// </remarks>
        public bool IsModified = false;

        /// <summary>
        /// Indicates whether this list is marked as deleted.
        /// </summary>
        /// <remarks>
        /// Effectively indicates whether the ListManager should delete it from disk (i.e. when the app exits).
        /// </remarks>
        public bool IsDeleted = false;

        // metadata header
        const string NameHeaderKey = "name";
        const string ReadOnlyHeaderKey = "readonly";

        /// <summary>
        /// Constructor for new lists where no dictionary file yet exists.
        /// </summary>
        /// <param name="name">The name of the list to be created.</param>
        public DictionaryRecordList(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Constructor for lists based on an existing dictionary file.
        /// </summary>
        /// <param name="dictionary">CC_CEDict.WindowsPhone instance containing the entries for the list.</param>
        public DictionaryRecordList(Dictionary dictionary)
        {
            foreach (DictionaryRecord record in dictionary)
                base.Add(record);

            try
            {
                Name = dictionary.Header[NameHeaderKey];
                ReadOnly = bool.Parse(dictionary.Header[ReadOnlyHeaderKey]);
            }
            catch (Exception)
            {
            }

            this.Sort();
            IsModified = false;
        }

        string _Name = "unknown";
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                IsModified = true;
            }
        }

        bool _ReadOnly = false;
        public bool ReadOnly
        {
            get
            {
                return _ReadOnly;
            }
            set
            {
                _ReadOnly = value;
                IsModified = true;
            }
        }

        public new void Add(DictionaryRecord record)
        {
            if (this.Contains(record))
                return;
            base.Add(record);
            this.Sort();
            IsModified = true;
        }

        public new void Remove(DictionaryRecord record)
        {
            base.Remove(record);
            IsModified = true;
        }

        string HeaderTemplate = "#! {0}={1}";
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format(HeaderTemplate, NameHeaderKey, this.Name));
            sb.AppendLine(String.Format(HeaderTemplate, ReadOnlyHeaderKey, this.ReadOnly));

            foreach (DictionaryRecord record in this)
                sb.AppendLine(record.ToString());

            return sb.ToString();
        }
    }
}
