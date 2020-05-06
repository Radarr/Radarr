namespace VersOne.Epub.Schema
{
    public enum StructuralSemanticsProperty
    {
        COVER = 1,
        FRONTMATTER,
        BODYMATTER,
        BACKMATTER,
        VOLUME,
        PART,
        CHAPTER,
        SUBCHAPTER,
        DIVISION,
        ABSTRACT,
        FOREWORD,
        PREFACE,
        PROLOGUE,
        INTRODUCTION,
        PREAMBLE,
        CONCLUSION,
        EPILOGUE,
        AFTERWORD,
        EPIGRAPH,
        TOC,
        TOC_BRIEF,
        LANDMARKS,
        LOA,
        LOI,
        LOT,
        LOV,
        APPENDIX,
        COLOPHON,
        CREDITS,
        KEYWORDS,
        INDEX,
        INDEX_HEADNOTES,
        INDEX_LEGEND,
        INDEX_GROUP,
        INDEX_ENTRY_LIST,
        INDEX_ENTRY,
        INDEX_TERM,
        INDEX_EDITOR_NOTE,
        INDEX_LOCATOR,
        INDEX_LOCATOR_LIST,
        INDEX_LOCATOR_RANGE,
        INDEX_XREF_PREFERRED,
        INDEX_XREF_RELATED,
        INDEX_TERM_CATEGORY,
        INDEX_TERM_CATEGORIES,
        GLOSSARY,
        GLOSSTERM,
        GLOSSDEF,
        BIBLIOGRAPHY,
        BIBLIOENTRY,
        TITLEPAGE,
        HALFTITLEPAGE,
        COPYRIGHT_PAGE,
        SERIESPAGE,
        ACKNOWLEDGMENTS,
        IMPRINT,
        IMPRIMATUR,
        CONTRIBUTORS,
        OTHER_CREDITS,
        ERRATA,
        DEDICATION,
        REVISION_HISTORY,
        CASE_STUDY,
        HELP,
        MARGINALIA,
        NOTICE,
        PULLQUOTE,
        SIDEBAR,
        TIP,
        WARNING,
        HALFTITLE,
        FULLTITLE,
        COVERTITLE,
        TITLE,
        SUBTITLE,
        LABEL,
        ORDINAL,
        BRIDGEHEAD,
        LEARNING_OBJECTIVE,
        LEARNING_OBJECTIVES,
        LEARNING_OUTCOME,
        LEARNING_OUTCOMES,
        LEARNING_RESOURCE,
        LEARNING_RESOURCES,
        LEARNING_STANDARD,
        LEARNING_STANDARDS,
        ANSWER,
        ANSWERS,
        ASSESSMENT,
        ASSESSMENTS,
        FEEDBACK,
        FILL_IN_THE_BLANK_PROBLEM,
        GENERAL_PROBLEM,
        QNA,
        MATCH_PROBLEM,
        MULTIPLE_CHOICE_PROBLEM,
        PRACTICE,
        QUESTION,
        PRACTICES,
        TRUE_FALSE_PROBLEM,
        PANEL,
        PANEL_GROUP,
        BALLOON,
        TEXT_AREA,
        SOUND_AREA,
        ANNOTATION,
        NOTE,
        FOOTNOTE,
        ENDNOTE,
        REARNOTE,
        FOOTNOTES,
        ENDNOTES,
        REARNOTES,
        ANNOREF,
        BIBLIOREF,
        GLOSSREF,
        NOTEREF,
        BACKLINK,
        CREDIT,
        KEYWORD,
        TOPIC_SENTENCE,
        CONCLUDING_SENTENCE,
        PAGEBREAK,
        PAGE_LIST,
        TABLE,
        TABLE_ROW,
        TABLE_CELL,
        LIST,
        LIST_ITEM,
        FIGURE,
        UNKNOWN
    }

    internal static class StructuralSemanticsPropertyParser
    {
        public static StructuralSemanticsProperty Parse(string stringValue)
        {
            switch (stringValue.ToLowerInvariant())
            {
                case "cover":
                    return StructuralSemanticsProperty.COVER;
                case "frontmatter":
                    return StructuralSemanticsProperty.FRONTMATTER;
                case "bodymatter":
                    return StructuralSemanticsProperty.BODYMATTER;
                case "backmatter":
                    return StructuralSemanticsProperty.BACKMATTER;
                case "volume":
                    return StructuralSemanticsProperty.VOLUME;
                case "part":
                    return StructuralSemanticsProperty.PART;
                case "chapter":
                    return StructuralSemanticsProperty.CHAPTER;
                case "subchapter":
                    return StructuralSemanticsProperty.SUBCHAPTER;
                case "division":
                    return StructuralSemanticsProperty.DIVISION;
                case "abstract":
                    return StructuralSemanticsProperty.ABSTRACT;
                case "foreword":
                    return StructuralSemanticsProperty.FOREWORD;
                case "preface":
                    return StructuralSemanticsProperty.PREFACE;
                case "prologue":
                    return StructuralSemanticsProperty.PROLOGUE;
                case "introduction":
                    return StructuralSemanticsProperty.INTRODUCTION;
                case "preamble":
                    return StructuralSemanticsProperty.PREAMBLE;
                case "conclusion":
                    return StructuralSemanticsProperty.CONCLUSION;
                case "epilogue":
                    return StructuralSemanticsProperty.EPILOGUE;
                case "afterword":
                    return StructuralSemanticsProperty.AFTERWORD;
                case "epigraph":
                    return StructuralSemanticsProperty.EPIGRAPH;
                case "toc":
                    return StructuralSemanticsProperty.TOC;
                case "toc-brief":
                    return StructuralSemanticsProperty.TOC_BRIEF;
                case "landmarks":
                    return StructuralSemanticsProperty.LANDMARKS;
                case "loa":
                    return StructuralSemanticsProperty.LOA;
                case "loi":
                    return StructuralSemanticsProperty.LOI;
                case "lot":
                    return StructuralSemanticsProperty.LOT;
                case "lov":
                    return StructuralSemanticsProperty.LOV;
                case "appendix":
                    return StructuralSemanticsProperty.APPENDIX;
                case "colophon":
                    return StructuralSemanticsProperty.COLOPHON;
                case "credits":
                    return StructuralSemanticsProperty.CREDITS;
                case "keywords":
                    return StructuralSemanticsProperty.KEYWORDS;
                case "index":
                    return StructuralSemanticsProperty.INDEX;
                case "index-headnotes":
                    return StructuralSemanticsProperty.INDEX_HEADNOTES;
                case "index-legend":
                    return StructuralSemanticsProperty.INDEX_LEGEND;
                case "index-group":
                    return StructuralSemanticsProperty.INDEX_GROUP;
                case "index-entry-list":
                    return StructuralSemanticsProperty.INDEX_ENTRY_LIST;
                case "index-entry":
                    return StructuralSemanticsProperty.INDEX_ENTRY;
                case "index-term":
                    return StructuralSemanticsProperty.INDEX_TERM;
                case "index-editor-note":
                    return StructuralSemanticsProperty.INDEX_EDITOR_NOTE;
                case "index-locator":
                    return StructuralSemanticsProperty.INDEX_LOCATOR;
                case "index-locator-list":
                    return StructuralSemanticsProperty.INDEX_LOCATOR_LIST;
                case "index-locator-range":
                    return StructuralSemanticsProperty.INDEX_LOCATOR_RANGE;
                case "index-xref-preferred":
                    return StructuralSemanticsProperty.INDEX_XREF_PREFERRED;
                case "index-xref-related":
                    return StructuralSemanticsProperty.INDEX_XREF_RELATED;
                case "index-term-category":
                    return StructuralSemanticsProperty.INDEX_TERM_CATEGORY;
                case "index-term-categories":
                    return StructuralSemanticsProperty.INDEX_TERM_CATEGORIES;
                case "glossary":
                    return StructuralSemanticsProperty.GLOSSARY;
                case "glossterm":
                    return StructuralSemanticsProperty.GLOSSTERM;
                case "glossdef":
                    return StructuralSemanticsProperty.GLOSSDEF;
                case "bibliography":
                    return StructuralSemanticsProperty.BIBLIOGRAPHY;
                case "biblioentry":
                    return StructuralSemanticsProperty.BIBLIOENTRY;
                case "titlepage":
                    return StructuralSemanticsProperty.TITLEPAGE;
                case "halftitlepage":
                    return StructuralSemanticsProperty.HALFTITLEPAGE;
                case "copyright-page":
                    return StructuralSemanticsProperty.COPYRIGHT_PAGE;
                case "seriespage":
                    return StructuralSemanticsProperty.SERIESPAGE;
                case "acknowledgments":
                    return StructuralSemanticsProperty.ACKNOWLEDGMENTS;
                case "imprint":
                    return StructuralSemanticsProperty.IMPRINT;
                case "imprimatur":
                    return StructuralSemanticsProperty.IMPRIMATUR;
                case "contributors":
                    return StructuralSemanticsProperty.CONTRIBUTORS;
                case "other-credits":
                    return StructuralSemanticsProperty.OTHER_CREDITS;
                case "errata":
                    return StructuralSemanticsProperty.ERRATA;
                case "dedication":
                    return StructuralSemanticsProperty.DEDICATION;
                case "revision-history":
                    return StructuralSemanticsProperty.REVISION_HISTORY;
                case "case-study":
                    return StructuralSemanticsProperty.CASE_STUDY;
                case "help":
                    return StructuralSemanticsProperty.HELP;
                case "marginalia":
                    return StructuralSemanticsProperty.MARGINALIA;
                case "notice":
                    return StructuralSemanticsProperty.NOTICE;
                case "pullquote":
                    return StructuralSemanticsProperty.PULLQUOTE;
                case "sidebar":
                    return StructuralSemanticsProperty.SIDEBAR;
                case "tip":
                    return StructuralSemanticsProperty.TIP;
                case "warning":
                    return StructuralSemanticsProperty.WARNING;
                case "halftitle":
                    return StructuralSemanticsProperty.HALFTITLE;
                case "fulltitle":
                    return StructuralSemanticsProperty.FULLTITLE;
                case "covertitle":
                    return StructuralSemanticsProperty.COVERTITLE;
                case "title":
                    return StructuralSemanticsProperty.TITLE;
                case "subtitle":
                    return StructuralSemanticsProperty.SUBTITLE;
                case "label":
                    return StructuralSemanticsProperty.LABEL;
                case "ordinal":
                    return StructuralSemanticsProperty.ORDINAL;
                case "bridgehead":
                    return StructuralSemanticsProperty.BRIDGEHEAD;
                case "learning-objective":
                    return StructuralSemanticsProperty.LEARNING_OBJECTIVE;
                case "learning-objectives":
                    return StructuralSemanticsProperty.LEARNING_OBJECTIVES;
                case "learning-outcome":
                    return StructuralSemanticsProperty.LEARNING_OUTCOME;
                case "learning-outcomes":
                    return StructuralSemanticsProperty.LEARNING_OUTCOMES;
                case "learning-resource":
                    return StructuralSemanticsProperty.LEARNING_RESOURCE;
                case "learning-resources":
                    return StructuralSemanticsProperty.LEARNING_RESOURCES;
                case "learning-standard":
                    return StructuralSemanticsProperty.LEARNING_STANDARD;
                case "learning-standards":
                    return StructuralSemanticsProperty.LEARNING_STANDARDS;
                case "answer":
                    return StructuralSemanticsProperty.ANSWER;
                case "answers":
                    return StructuralSemanticsProperty.ANSWERS;
                case "assessment":
                    return StructuralSemanticsProperty.ASSESSMENT;
                case "assessments":
                    return StructuralSemanticsProperty.ASSESSMENTS;
                case "feedback":
                    return StructuralSemanticsProperty.FEEDBACK;
                case "fill-in-the-blank-problem":
                    return StructuralSemanticsProperty.FILL_IN_THE_BLANK_PROBLEM;
                case "general-problem":
                    return StructuralSemanticsProperty.GENERAL_PROBLEM;
                case "qna":
                    return StructuralSemanticsProperty.QNA;
                case "match-problem":
                    return StructuralSemanticsProperty.MATCH_PROBLEM;
                case "multiple-choice-problem":
                    return StructuralSemanticsProperty.MULTIPLE_CHOICE_PROBLEM;
                case "practice":
                    return StructuralSemanticsProperty.PRACTICE;
                case "question":
                    return StructuralSemanticsProperty.QUESTION;
                case "practices":
                    return StructuralSemanticsProperty.PRACTICES;
                case "true-false-problem":
                    return StructuralSemanticsProperty.TRUE_FALSE_PROBLEM;
                case "panel":
                    return StructuralSemanticsProperty.PANEL;
                case "panel-group":
                    return StructuralSemanticsProperty.PANEL_GROUP;
                case "balloon":
                    return StructuralSemanticsProperty.BALLOON;
                case "text-area":
                    return StructuralSemanticsProperty.TEXT_AREA;
                case "sound-area":
                    return StructuralSemanticsProperty.SOUND_AREA;
                case "annotation":
                    return StructuralSemanticsProperty.ANNOTATION;
                case "note":
                    return StructuralSemanticsProperty.NOTE;
                case "footnote":
                    return StructuralSemanticsProperty.FOOTNOTE;
                case "endnote":
                    return StructuralSemanticsProperty.ENDNOTE;
                case "rearnote":
                    return StructuralSemanticsProperty.REARNOTE;
                case "footnotes":
                    return StructuralSemanticsProperty.FOOTNOTES;
                case "endnotes":
                    return StructuralSemanticsProperty.ENDNOTES;
                case "rearnotes":
                    return StructuralSemanticsProperty.REARNOTES;
                case "annoref":
                    return StructuralSemanticsProperty.ANNOREF;
                case "biblioref":
                    return StructuralSemanticsProperty.BIBLIOREF;
                case "glossref":
                    return StructuralSemanticsProperty.GLOSSREF;
                case "noteref":
                    return StructuralSemanticsProperty.NOTEREF;
                case "backlink":
                    return StructuralSemanticsProperty.BACKLINK;
                case "credit":
                    return StructuralSemanticsProperty.CREDIT;
                case "keyword":
                    return StructuralSemanticsProperty.KEYWORD;
                case "topic-sentence":
                    return StructuralSemanticsProperty.TOPIC_SENTENCE;
                case "concluding-sentence":
                    return StructuralSemanticsProperty.CONCLUDING_SENTENCE;
                case "pagebreak":
                    return StructuralSemanticsProperty.PAGEBREAK;
                case "page-list":
                    return StructuralSemanticsProperty.PAGE_LIST;
                case "table":
                    return StructuralSemanticsProperty.TABLE;
                case "table-row":
                    return StructuralSemanticsProperty.TABLE_ROW;
                case "table-cell":
                    return StructuralSemanticsProperty.TABLE_CELL;
                case "list":
                    return StructuralSemanticsProperty.LIST;
                case "list-item":
                    return StructuralSemanticsProperty.LIST_ITEM;
                case "figure":
                    return StructuralSemanticsProperty.FIGURE;
                default:
                    return StructuralSemanticsProperty.UNKNOWN;
            }
        }
    }
}
