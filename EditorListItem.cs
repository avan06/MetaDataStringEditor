using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetaDataStringEditor {

    public class EditorListItem : ListViewItem {

        public bool IsEdit { private set; get; }
        public byte[] OriginStrBytes { private set; get; }
        public byte[] NewStrBytes { private set; get; }

        public EditorListItem(byte[] OriginStrBytes) {
            this.OriginStrBytes = OriginStrBytes;
            IsEdit = false;

            Text = (string)Tag;
            SubItems.Add(Encoding.UTF8.GetString(OriginStrBytes));
            SubItems.Add("");
            SubItems.Add("");
        }

        public void SetNewStr(string newString) {
            NewStrBytes = Encoding.UTF8.GetBytes(newString);
            IsEdit = !Equals(OriginStrBytes, NewStrBytes);

            SubItems[2].Text = IsEdit ? newString : "";
            SubItems[3].Text = IsEdit ? "*" : "";
        }

        public void Discard() {
            NewStrBytes = null;
            IsEdit = false;

            SubItems[2].Text = "";
            SubItems[3].Text = "";
        }

        public bool MatchKeyWord(string keyWord) {
            return Text.ToLower().Contains(keyWord.ToLower()) ||
                SubItems[1].Text.ToLower().Contains(keyWord.ToLower()) ||
                SubItems[2].Text.ToLower().Contains(keyWord.ToLower()) ||
                SubItems[3].Text.ToLower().Contains(keyWord.ToLower());
        }
    }
}
