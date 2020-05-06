using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NzbDrone.Core.MediaFiles.Azw
{
    public struct SectionInfo
    {
        public ulong Start_addr;
        public ulong End_addr;

        public ulong Length => End_addr - Start_addr;
    }

    public class AzwFile
    {
        public byte[] Raw_data;
        public ushort Section_count;
        public SectionInfo[] Section_info;
        public string Ident;

        protected AzwFile(string path)
        {
            Raw_data = File.ReadAllBytes(path);
            GetSectionInfo();

            if (Ident != "BOOKMOBI" || Section_count == 0)
            {
                throw new AzwTagException("Invalid mobi header");
            }
        }

        protected void GetSectionInfo()
        {
            Ident = Encoding.ASCII.GetString(Raw_data, 0x3c, 8);
            Section_count = Util.GetUInt16(Raw_data, 76);
            Section_info = new SectionInfo[Section_count];

            Section_info[0].Start_addr = Util.GetUInt32(Raw_data, 78);
            for (uint i = 1; i < Section_count; i++)
            {
                Section_info[i].Start_addr = Util.GetUInt32(Raw_data, 78 + (i * 8));
                Section_info[i - 1].End_addr = Section_info[i].Start_addr;
            }

            Section_info[Section_count - 1].End_addr = (ulong)Raw_data.Length;
        }

        protected byte[] GetSectionData(uint i)
        {
            return Util.SubArray(Raw_data, Section_info[i].Start_addr, Section_info[i].Length);
        }
    }

    public class IdMapping
    {
        public static Dictionary<uint, string> Id_map_strings = new Dictionary<uint, string>
        {
           { 1, "Drm Server Id (1)" },
           { 2, "Drm Commerce Id (2)" },
           { 3, "Drm Ebookbase Book Id(3)" },
           { 100, "Creator_(100)" },
           { 101, "Publisher_(101)" },
           { 102, "Imprint_(102)" },
           { 103, "Description_(103)" },
           { 104, "ISBN_(104)" },
           { 105, "Subject_(105)" },
           { 106, "Published_(106)" },
           { 107, "Review_(107)" },
           { 108, "Contributor_(108)" },
           { 109, "Rights_(109)" },
           { 110, "SubjectCode_(110)" },
           { 111, "Type_(111)" },
           { 112, "Source_(112)" },
           { 113, "ASIN_(113)" },
           { 114, "versionNumber_(114)" },
           { 117, "Adult_(117)" },
           { 118, "Price_(118)" },
           { 119, "Currency_(119)" },
           { 122, "fixed-layout_(122)" },
           { 123, "book-type_(123)" },
           { 124, "orientation-lock_(124)" },
           { 126, "original-resolution_(126)" },
           { 127, "zero-gutter_(127)" },
           { 128, "zero-margin_(128)" },
           { 129, "K8_Masthead/Cover_Image_(129)" },
           { 132, "RegionMagnification_(132)" },
           { 200, "DictShortName_(200)" },
           { 208, "Watermark_(208)" },
           { 501, "cdeType_(501)" },
           { 502, "last_update_time_(502)" },
           { 503, "Updated_Title_(503)" },
           { 504, "ASIN_(504)" },
           { 508, "Title_Katagana_(508)" },
           { 517, "Creator_Katagana_(517)" },
           { 522, "Publisher_Katagana_(522)" },
           { 524, "Language_(524)" },
           { 525, "primary-writing-mode_(525)" },
           { 526, "Unknown_(526)" },
           { 527, "page-progression-direction_(527)" },
           { 528, "override-kindle_fonts_(528)" },
           { 529, "Unknown_(529)" },
           { 534, "Input_Source_Type_(534)" },
           { 535, "Kindlegen_BuildRev_Number_(535)" },
           { 536, "Container_Info_(536)" }, // CONT_Header is 0, Ends with CONTAINER_BOUNDARY (or Asset_Type?)
           { 538, "Container_Resolution_(538)" },
           { 539, "Container_Mimetype_(539)" },
           { 542, "Unknown_but_changes_with_filename_only_(542)" },
           { 543, "Container_id_(543)" },  // FONT_CONTAINER, BW_CONTAINER, HD_CONTAINER
           { 544, "Unknown_(544)" }
        };

        public static Dictionary<uint, string> Id_map_values = new Dictionary<uint, string>()
        {
           { 115, "sample_(115)" },
           { 116, "StartOffset_(116)" },
           { 121, "K8(121)_Boundary_Section_(121)" },
           { 125, "K8_Count_of_Resources_Fonts_Images_(125)" },
           { 131, "K8_Unidentified_Count_(131)" },
           { 201, "CoverOffset_(201)" },
           { 202, "ThumbOffset_(202)" },
           { 203, "Fake_Cover_(203)" },
           { 204, "Creator_Software_(204)" },
           { 205, "Creator_Major_Version_(205)" },
           { 206, "Creator_Minor_Version_(206)" },
           { 207, "Creator_Build_Number_(207)" },
           { 401, "Clipping_Limit_(401)" },
           { 402, "Publisher_Limit_(402)" },
           { 404, "Text_to_Speech_Disabled_(404)" },
           { 406, "Rental_Indicator_(406)" }
        };

        public static Dictionary<uint, string> Id_map_hex = new Dictionary<uint, string>()
        {
            { 208, "Watermark(208 in hex)" },
            { 209, "Tamper_Proof_Keys_(209_in_hex)" },
            { 300, "Font_Signature_(300_in_hex)" }
        };
    }
}
