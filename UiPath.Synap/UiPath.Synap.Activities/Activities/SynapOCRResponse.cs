using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace UiPathTeams.Synap.OCR
{
    enum SynapFieldType
    {
        DEFAULT = 1,
        BLOCK = 2,
        LINE = 4
    }
    public class SynapContext
    {
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
    }

    public class SynapOCRField
    {
        public List<Point> Points { get; } = new List<Point>();

        public double Confidence { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return string.Join(", ", Points) + $", confidence: {Confidence}, Text: {Text}";
        }
    }

    public class SynapOCRResult
    {
        public string fid { get; set; }
        public double dur { get; set; }
        public string csv_file_name { get; set; }
        public string full_text { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string final_file_name { get; set; }
        public int total_page { get; set; }
        public int page_index { get; set; }
        public double rotate { get; set; }

        private void parseFields(List<SynapOCRField> fields, List<object> items)
        {
            foreach (var obj in items)
            {
                var field = new SynapOCRField();
                JArray arr = JArray.Parse(obj.ToString());
                for (int i = 0; i < 4; i++)
                {
                    var tmp = JArray.Parse(arr[i].ToString());
                    field.Points.Add(new Point((int)Double.Parse(tmp[0].ToString()), (int)Double.Parse(tmp[1].ToString())));
                }
                field.Confidence = Double.Parse(arr[4].ToString());
                field.Text = arr[5].ToString();
                fields.Add(field);
            }
        }
        public SynapOCRField[] GetFields(int type)
        {
            if (type == (int)SynapFieldType.DEFAULT)
            {
                if (this._fields != null)
                    return this._fields.ToArray();
                this._fields = new List<SynapOCRField>();
                parseFields(this._fields, this.boxes);
                return this._fields.ToArray();
            }
            else if (type == (int)SynapFieldType.BLOCK)
            {
                if (this._block_fields != null)
                    return this._block_fields.ToArray();
                this._block_fields = new List<SynapOCRField>();
                parseFields(this._block_fields, this.block_boxes);
                return this._block_fields.ToArray();
            }
            else if (type == (int)SynapFieldType.LINE)
            {
                if (this._line_fields != null)
                    return this._line_fields.ToArray();
                this._line_fields = new List<SynapOCRField>();
                parseFields(this._line_fields, this.line_boxes);
                this._line_fields.ToArray();
            }
            return null;
        }

        private List<SynapOCRField> _fields;
        private List<SynapOCRField> _block_fields;
        private List<SynapOCRField> _line_fields;

        public List<object> boxes { get; set; }
        public List<object> block_boxes { get; set; }
        public List<object> line_boxes { get; set; }

        public override string ToString()
        {
            return $"FID: {fid}, full_text: {full_text},  Dur: {dur}, height: {height}, width: {width}, total_page: {total_page}, page_index: {page_index}, final_file_name: {final_file_name}, csv_file_name: {csv_file_name} rotate: {rotate} boxes: {boxes} ";
        }

    }
    public class SynapOCRResponse
    {
        public int status { get; set; }
        public SynapOCRResult result { get; set; }

        public override string ToString()
        {
            return $"status: {status}, result : {result}";
        }
    }

    public class SynapOCRResponseConverter : CustomCreationConverter<SynapOCRResponse>
    {
        public override SynapOCRResponse Create(Type objectType)
        {
            return new SynapOCRResponse();
        }
    }
}
