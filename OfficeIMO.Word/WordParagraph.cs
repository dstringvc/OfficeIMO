﻿using System;
using System.Collections.Generic;
using System.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Office2013.Drawing.TimeSlicer;
using DocumentFormat.OpenXml.Packaging;

namespace OfficeIMO.Word {
    public partial class WordParagraph {
        internal WordDocument _document;
        internal Paragraph _paragraph;
        internal RunProperties _runProperties;
        internal Text _text;
        internal Run _run;
        internal ParagraphProperties _paragraphProperties;
        internal WordParagraph _linkedParagraph;
        internal WordSection _section;

        public WordImage Image { get; set; }

        public bool IsListItem {
            get {
                if (_paragraphProperties != null && _paragraphProperties.NumberingProperties != null) {
                    return true;
                } else {
                    return false;
                }
            }
        }

        public int? ListItemLevel {
            get {
                if (_paragraphProperties != null && _paragraphProperties.NumberingProperties != null) {
                    return _paragraphProperties.NumberingProperties.NumberingLevelReference.Val;
                } else {
                    return null;
                }
            }
            set {
                if (_paragraphProperties != null && _paragraphProperties.NumberingProperties != null) {
                    if (_paragraphProperties.NumberingProperties.NumberingLevelReference != null) {
                        _paragraphProperties.NumberingProperties.NumberingLevelReference.Val = value;
                    }
                } else {
                    // should throw?
                }
            }
        }

        internal int? _listNumberId {
            get {
                if (_paragraphProperties != null && _paragraphProperties.NumberingProperties != null) {
                    return _paragraphProperties.NumberingProperties.NumberingId.Val;
                } else {
                    return null;
                }
            }
        }


        public WordParagraphStyles? Style {
            get {
                if (_paragraphProperties != null && _paragraphProperties.ParagraphStyleId != null) {
                    return WordParagraphStyle.GetStyle(_paragraphProperties.ParagraphStyleId.Val);
                }

                return null;
            }
            set {
                if (value != null) {
                    if (_paragraphProperties == null) {
                        _paragraphProperties = new ParagraphProperties();
                    }

                    if (_paragraphProperties.ParagraphStyleId == null) {
                        _paragraphProperties.ParagraphStyleId = new ParagraphStyleId();
                    }

                    _paragraphProperties.ParagraphStyleId.Val = value.Value.ToStringStyle();
                }
            }
        }


        internal WordList _list;

        public string Text {
            get {
                if (_text == null) {
                    return "";
                }

                return _text.Text;
            }
            set {
                VerifyText();
                _text.Text = value;
            }
        }

        public Run VerifyRun() {
            if (this._run == null) {
                this._run = new Run();
                this._paragraph.Append(_run);
            }
            return this._run;
        }

        public Text VerifyText() {
            if (_text == null) {
                var run = VerifyRun();

                this._text = new Text {
                    // this ensures spaces are preserved between runs
                    Space = SpaceProcessingModeValues.Preserve
                };
                this._run.Append(_text);
            }

            return this._text;
        }

        public WordParagraph(WordSection section, bool newParagraph = true) {
            this._document = section._document;
            // this._section = section;

            this._run = new Run();
            this._runProperties = new RunProperties();

            //this._run = new Run();
            //this._runProperties = new RunProperties();
            this._text = new Text {
                // this ensures spaces are preserved between runs
                Space = SpaceProcessingModeValues.Preserve
            };
            this._paragraphProperties = new ParagraphProperties();
            this._run.AppendChild(_runProperties);
            this._run.AppendChild(_text);
            if (newParagraph) {
                this._paragraph = new Paragraph();
                this._paragraph.AppendChild(_paragraphProperties);
                this._paragraph.AppendChild(_run);
            }

            //section.Paragraphs.Add(this);
        }

        public WordParagraph(WordDocument document = null, bool newParagraph = true) {
            this._document = document;
            this._run = new Run();
            this._runProperties = new RunProperties();
            this._text = new Text {
                // this ensures spaces are preserved between runs
                Space = SpaceProcessingModeValues.Preserve
            };
            this._paragraphProperties = new ParagraphProperties();
            this._run.AppendChild(_runProperties);
            this._run.AppendChild(_text);
            if (newParagraph) {
                this._paragraph = new Paragraph();
                this._paragraph.AppendChild(_paragraphProperties);
                this._paragraph.AppendChild(_run);
            }

            if (document != null) {
                //  document._currentSection.Paragraphs.Add(this);
                //this._section = document._currentSection;
                //document.Paragraphs.Add(this);
            }
        }

        public WordParagraph(WordDocument document, bool newParagraph, Paragraph paragraph, ParagraphProperties paragraphProperties, RunProperties runProperties, Run run, WordSection section = null) {
            this._document = document;
            this._section = section;
            this._run = run;
            this._runProperties = runProperties;
            this._paragraph = paragraph;
            //this._text = new Text {
            //    // this ensures spaces are preserved between runs
            //    Space = SpaceProcessingModeValues.Preserve
            //};

            if (run != null) this._text = run.OfType<Text>().FirstOrDefault();
            this._paragraphProperties = paragraphProperties;
            if (this._run != null) {
                //  this._run.AppendChild(_runProperties);
                // this._run.AppendChild(_text);
            }

            if (newParagraph) {
                this._paragraph = new Paragraph();

                if (_paragraphProperties != null) {
                    //this._paragraph.ParagraphProperties = _paragraphProperties;
                    this._paragraph.AppendChild(_paragraphProperties);
                }
                if (_run != null) this._paragraph.AppendChild(_run);
            }

            if (document != null) {
                // document._currentSection.Paragraphs.Add(this);
                //section.Paragraphs.Add(this);
                //document.Paragraphs.Add(this);
            }
        }

        //public WordParagraph(string text) {
        //    WordParagraph paragraph = new WordParagraph(this._document);
        //    paragraph.Text = text;
        //}

        /// <summary>
        /// Used during loading of documents / tables only
        /// </summary>
        /// <param name="document"></param>
        /// <param name="paragraph"></param>
        /// <param name="section"></param>
        public WordParagraph(WordDocument document, Paragraph paragraph, WordSection section = null) {
            //_paragraph = paragraph;
            if (paragraph.ParagraphProperties != null && paragraph.ParagraphProperties.SectionProperties != null) {
                // TODO this means it's a section and we don't want to add sections to paragraphs don't we?

                this._paragraph = paragraph;
                return;
            }

            int count = 0;
            var listRuns = paragraph.ChildElements.OfType<Run>();
            if (listRuns.Any()) {
                foreach (var run in paragraph.ChildElements.OfType<Run>()) {
                    RunProperties runProperties = run.RunProperties;
                    Text text = run.ChildElements.OfType<Text>().FirstOrDefault();
                    Drawing drawing = run.ChildElements.OfType<Drawing>().FirstOrDefault();

                    WordImage newImage = null;
                    if (drawing != null) {
                        newImage = new WordImage(document, drawing);
                    }

                    if (count > 0) {
                        WordParagraph wordParagraph = new WordParagraph(this._document);
                        wordParagraph._document = document;
                        wordParagraph._run = run;
                        wordParagraph._text = text;
                        wordParagraph._paragraph = paragraph;
                        wordParagraph._paragraphProperties = paragraph.ParagraphProperties;
                        wordParagraph._runProperties = runProperties;
                        wordParagraph._section = section;

                        wordParagraph.Image = newImage;

                        //document._currentSection.Paragraphs.Add(wordParagraph);
                        if (wordParagraph.IsPageBreak) {
                            document._currentSection.PageBreaks.Add(wordParagraph);
                        }

                        if (wordParagraph.IsListItem) {
                            LoadListToDocument(document, wordParagraph);
                        }
                    } else {
                        this._document = document;
                        this._run = run;
                        this._text = text;
                        this._paragraph = paragraph;
                        this._paragraphProperties = paragraph.ParagraphProperties;
                        this._runProperties = runProperties;
                        this._section = section;

                        if (newImage != null) {
                            this.Image = newImage;
                        }

                        // this is to prevent adding Tables Paragraphs to section Paragraphs
                        if (section != null) {
                            section.Paragraphs.Add(this);
                            if (this.IsPageBreak) {
                                section.PageBreaks.Add(this);
                            }
                        }

                        if (this.IsListItem) {
                            LoadListToDocument(document, this);
                        }
                    }

                    count++;
                }
            } else {
                // this is an empty paragraph so we add it
                document._currentSection.Paragraphs.Add(this);
                this._section = document._currentSection;
            }
        }

        private void LoadListToDocument(WordDocument document, WordParagraph wordParagraph) {
            if (wordParagraph.IsListItem) {
                int? listId = wordParagraph._listNumberId;
                if (listId != null) {
                    if (!_document._listNumbersUsed.Contains(listId.Value)) {
                        WordList list = new WordList(wordParagraph._document, document._currentSection, listId.Value);
                        list.ListItems.Add(wordParagraph);
                        _document._listNumbersUsed.Add(listId.Value);
                        _document._currentSection.Lists.Add(list);
                    } else {
                        foreach (WordList list in _document.Lists) {
                            if (list._numberId == listId.Value) {
                                list.ListItems.Add(wordParagraph);
                            }
                        }
                    }
                } else {
                    throw new InvalidOperationException("Couldn't load a list, probably some logic error :-)");
                }
            }
        }

        public WordParagraph AddText(string text) {
            WordParagraph wordParagraph = new WordParagraph(this._document, false);
            wordParagraph.Text = text;

            // this ensures that we keep track of matching runs with real paragraphs
            wordParagraph._linkedParagraph = this;

            if (this._linkedParagraph != null) {
                this._linkedParagraph._paragraph.Append(wordParagraph._run);
            } else {
                this._paragraph.Append(wordParagraph._run);
            }

            //this._document._wordprocessingDocument.MainDocumentPart.Document.InsertAfter(wordParagraph._run, this._paragraph);
            return wordParagraph;
        }

        public WordParagraph AddImage(string filePathImage, double? width, double? height) {
            WordImage wordImage = new WordImage(this._document, filePathImage, width, height);
            WordParagraph paragraph = new WordParagraph(this._document);
            _run.Append(wordImage._Image);
            this.Image = wordImage;
            return paragraph;
        }

        public WordParagraph AddImage(string filePathImage) {
            WordImage wordImage = new WordImage(this._document, filePathImage, null, null);
            WordParagraph paragraph = new WordParagraph(this._document);
            _run.Append(wordImage._Image);
            this.Image = wordImage;
            return paragraph;
        }

        public void Remove() {
            if (_paragraph != null) {
                if (this._paragraph.Parent != null) {
                    this._paragraph.Remove();
                } else {
                    throw new InvalidOperationException("This shouldn't happen? Why? Oh why?");
                    //Console.WriteLine(this._run);
                }
            } else {
                // this happens if we continue adding to real paragraphs additional runs. In this case we don't need to,
                // delete paragraph, but only remove Runs 
                this._run.Remove();
            }

            if (IsPageBreak) {
                this._document.PageBreaks.Remove(this);
            }

            if (IsListItem) {
                if (this._list != null) {
                    this._list.ListItems.Remove(this);
                    this._list = null;
                }
            }

            this._document.Paragraphs.Remove(this);
        }

        public WordParagraph AddParagraphAfterSelf() {
            WordParagraph paragraph = new WordParagraph(this._document, true);
            this._paragraph.InsertAfterSelf(paragraph._paragraph);
            //this._document.Paragraphs.Add(paragraph);

            return paragraph;
        }

        public WordParagraph AddParagraphAfterSelf(WordSection section) {
            //WordParagraph paragraph = new WordParagraph(section._document, true);
            WordParagraph paragraph = new WordParagraph(section, true);

            this._paragraph.InsertAfterSelf(paragraph._paragraph);
            //this._document.Paragraphs.Add(paragraph);

            return paragraph;
        }

        public WordParagraph AddParagraphBeforeSelf() {
            WordParagraph paragraph = new WordParagraph(this._document, true);
            this._paragraph.InsertBeforeSelf(paragraph._paragraph);
            //document.Paragraphs.Add(paragraph);
            return paragraph;
        }

        /// <summary>
        /// Add a comment to paragraph
        /// </summary>
        /// <param name="author"></param>
        /// <param name="initials"></param>
        /// <param name="comment"></param>
        public void AddComment(string author, string initials, string comment) {
            Comments comments = null;
            string id = "0";

            // Verify that the document contains a 
            // WordProcessingCommentsPart part; if not, add a new one.
            if (this._document._wordprocessingDocument.MainDocumentPart.GetPartsCountOfType<WordprocessingCommentsPart>() > 0) {
                comments = this._document._wordprocessingDocument.MainDocumentPart.WordprocessingCommentsPart.Comments;
                if (comments.HasChildren) {
                    // Obtain an unused ID.
                    id = (comments.Descendants<Comment>().Select(e => int.Parse(e.Id.Value)).Max() + 1).ToString();
                }
            } else {
                // No WordprocessingCommentsPart part exists, so add one to the package.
                WordprocessingCommentsPart commentPart = this._document._wordprocessingDocument.MainDocumentPart.AddNewPart<WordprocessingCommentsPart>();
                commentPart.Comments = new Comments();
                comments = commentPart.Comments;
            }

            // Compose a new Comment and add it to the Comments part.
            Paragraph p = new Paragraph(new Run(new Text(comment)));
            Comment cmt =
                new Comment() {
                    Id = id,
                    Author = author,
                    Initials = initials,
                    Date = DateTime.Now
                };
            cmt.AppendChild(p);
            comments.AppendChild(cmt);
            comments.Save();

            // Specify the text range for the Comment. 
            // Insert the new CommentRangeStart before the first run of paragraph.
            this._paragraph.InsertBefore(new CommentRangeStart() { Id = id }, this._paragraph.GetFirstChild<Run>());

            // Insert the new CommentRangeEnd after last run of paragraph.
            var cmtEnd = this._paragraph.InsertAfter(new CommentRangeEnd() { Id = id }, this._paragraph.Elements<Run>().Last());

            // Compose a run with CommentReference and insert it.
            this._paragraph.InsertAfter(new Run(new CommentReference() { Id = id }), cmtEnd);
        }

        /// <summary>
        /// Add horizontal line (sometimes known as horizontal rule) to document
        /// </summary>
        /// <param name="lineType"></param>
        /// <param name="color"></param>
        /// <param name="size"></param>
        /// <param name="space"></param>
        /// <returns></returns>
        public WordParagraph AddHorizontalLine(BorderValues lineType = BorderValues.Single, System.Drawing.Color? color = null, uint size = 12, uint space = 1) {
            this._paragraphProperties.ParagraphBorders = new ParagraphBorders();
            this._paragraphProperties.ParagraphBorders.BottomBorder = new BottomBorder() {
                Val = lineType,
                Size = size,
                Space = space,
                Color = color != null ? color.Value.ToHexColor() : "auto"
            };

            //newWordParagraph._paragraph = new Paragraph(newWordParagraph._paragraphProperties);

            //this._document._wordprocessingDocument.MainDocumentPart.Document.Body.Append(this._paragraph);
            //this._currentSection.PageBreaks.Add(newWordParagraph);
            //this._currentSection.Paragraphs.Add(newWordParagraph);
            return this;
        }
    }
}