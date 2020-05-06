using System.Collections.Generic;

namespace NzbDrone.Core.Books.Calibre
{
    public enum CalibreFormat
    {
        None,
        EPUB,
        AZW3,
        MOBI,
        DOCX,
        FB2,
        HTMLZ,
        LIT,
        LRF,
        PDB,
        PDF,
        PMLZ,
        RB,
        RTF,
        SNB,
        TCR,
        TXT,
        TXTZ,
        ZIP
    }

    public enum CalibreProfile
    {
        Default,
        cybookg3,
        cybook_opus,
        generic_eink,
        generic_eink_hd,
        generic_eink_large,
        hanlinv3,
        hanlinv5,
        illiad,
        ipad,
        ipad3,
        irexdr1000,
        irexdr800,
        jetbook5,
        kindle,
        kindle_dx,
        kindle_fire,
        kindle_oasis,
        kindle_pw,
        kindle_pw3,
        kindle_voyage,
        kobo,
        msreader,
        mobipocket,
        nook,
        nook_color,
        nook_hd_plus,
        pocketbook_900,
        pocketbook_pro_912,
        galaxy,
        sony,
        sony300,
        sony900,
        sony_landscape,
        sonyt3,
        tablet
    }

    public class CalibreBookData
    {
        public CalibreConversionOptions Conversion_options { get; set; }
        public int Book_id { get; set; }
        public List<string> Input_formats { get; set; }
        public List<string> Output_formats { get; set; }
    }

    public class CalibreConversionOptions
    {
        public CalibreOptions Options { get; set; }
        public string Input_fmt { get; set; }
        public string Output_fmt { get; set; }
    }

    public class CalibreOptions
    {
        public string Output_profile { get; set; }
    }
}
