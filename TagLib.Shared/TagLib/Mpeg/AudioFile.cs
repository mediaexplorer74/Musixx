//
// AudioFile.cs: Provides tagging and properties support for MPEG-1, MPEG-2, and
// MPEG-2.5 audio files.
//

using System;
using System.Diagnostics;

namespace TagLib.Mpeg {
	/// <summary>
	///    This class extends <see cref="TagLib.NonContainer.File" /> to
	///    provide tagging and properties support for MPEG-1, MPEG-2, and
	///    MPEG-2.5 audio files.
	/// </summary>
	/// <remarks>
	///    A <see cref="TagLib.Id3v1.Tag" /> and <see
	///    cref="TagLib.Id3v2.Tag" /> will be added automatically to any
	///    file that doesn't contain one. This change does not effect the
	///    file until it is saved and can be reversed using the following
	///    method:
	///    <code>file.RemoveTags (file.TagTypes &amp; ~file.TagTypesOnDisk);</code>
	/// </remarks>
	[SupportedMimeType("taglib/mp3", "mp3")]
	[SupportedMimeType("audio/x-mp3")]
	[SupportedMimeType("application/x-id3")]
	[SupportedMimeType("audio/mpeg")]
	[SupportedMimeType("audio/x-mpeg")]
	[SupportedMimeType("audio/x-mpeg-3")]
	[SupportedMimeType("audio/mpeg3")]
	[SupportedMimeType("audio/mp3")]
	[SupportedMimeType("taglib/m2a", "m2a")]
	[SupportedMimeType("taglib/mp2", "mp2")]
	[SupportedMimeType("taglib/mp1", "mp1")]
	[SupportedMimeType("audio/x-mp2")]
	[SupportedMimeType("audio/x-mp1")]
	public class AudioFile : TagLib.NonContainer.File
	{
		#region Private Fields
		
		/// <summary>
		///    Contains the first audio header.
		/// </summary>
		private AudioHeader first_header;
		
		#endregion
		
		#region Constructors
		/// <summary>
		///    Constructs and initializes a new instance of <see
		///    cref="AudioFile" /> for a specified file abstraction and
		///    specified read style.
		/// </summary>
		/// <param name="abstraction">
		///    A <see cref="TagLib.File.IFileAbstraction" /> object to use when
		///    reading from and writing to the file.
		/// </param>
		/// <param name="propertiesStyle">
		///    A <see cref="ReadStyle" /> value specifying at what level
		///    of accuracy to read the media properties, or <see
		///    cref="ReadStyle.None" /> to ignore the properties.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="abstraction" /> is <see langword="null"
		///    />.
		/// </exception>
		public AudioFile (File.IFileAbstraction abstraction,
		             ReadStyle propertiesStyle)
			: base (abstraction, propertiesStyle)
		{
		}
		
		/// <summary>
		///    Constructs and initializes a new instance of <see
		///    cref="AudioFile" /> for a specified file abstraction with
		///    an average read style.
		/// </summary>
		/// <param name="abstraction">
		///    A <see cref="TagLib.File.IFileAbstraction" /> object to use when
		///    reading from and writing to the file.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///    <paramref name="abstraction" /> is <see langword="null"
		///    />.
		/// </exception>
		public AudioFile (File.IFileAbstraction abstraction)
			: base (abstraction)
		{
		}
		
		#endregion
		
		
		
		#region Public Methods
		
		/// <summary>
		///    Gets a tag of a specified type from the current instance,
		///    optionally creating a new tag if possible.
		/// </summary>
		/// <param name="type">
		///    A <see cref="TagLib.TagTypes" /> value indicating the
		///    type of tag to read.
		/// </param>
		/// <param name="create">
		///    A <see cref="bool" /> value specifying whether or not to
		///    try and create the tag if one is not found.
		/// </param>
		/// <returns>
		///    A <see cref="Tag" /> object containing the tag that was
		///    found in or added to the current instance. If no
		///    matching tag was found and none was created, <see
		///    langword="null" /> is returned.
		/// </returns>
		/// <remarks>
		///    If a <see cref="TagLib.Id3v2.Tag" /> is added to the
		///    current instance, it will be placed at the start of the
		///    file. On the other hand, <see cref="TagLib.Id3v1.Tag" />
		///    <see cref="TagLib.Ape.Tag" /> will be added to the end of
		///    the file. All other tag types will be ignored.
		/// </remarks>
		public override TagLib.Tag GetTag (TagTypes type, bool create)
		{
			Tag t = (Tag as TagLib.NonContainer.Tag).GetTag (type);
			
			if (t != null || !create)
				return t;
			
			switch (type)
			{
			case TagTypes.Id3v1:
				return EndTag.AddTag (type, Tag);
			
			case TagTypes.Id3v2:
				return StartTag.AddTag (type, Tag);
			
			case TagTypes.Ape:
				return EndTag.AddTag (type, Tag);
			
			default:
				return null;
			}
		}
		
		#endregion
		
		
		
		#region Protected Methods
		
		/// <summary>
		///    Reads format specific information at the start of the
		///    file.
		/// </summary>
		/// <param name="start">
		///    A <see cref="long" /> value containing the seek position
		///    at which the tags end and the media data begins.
		/// </param>
		/// <param name="propertiesStyle">
		///    A <see cref="ReadStyle" /> value specifying at what level
		///    of accuracy to read the media properties, or <see
		///    cref="ReadStyle.None" /> to ignore the properties.
		/// </param>
		/// <remarks>
		///    This method only searches for an audio header in the
		///    first 16384 bytes of code to avoid searching forever in
		///    corrupt files.
		/// </remarks>
		protected override void ReadStart (long start,
		                                   ReadStyle propertiesStyle)
		{
			// Only check the first 16 bytes so we're not stuck
			// reading a bad file forever.
			if (propertiesStyle != ReadStyle.None &&
				!AudioHeader.Find(out first_header, this,
					start, 0x4000))
			{
				//throw new CorruptFileException(
				//	"MPEG audio header not found.");
				Debug.WriteLine("TagLib exception: MPEG audio header not found.");
			}
		}
		
		/// <summary>
		///    Reads format specific information at the end of the
		///    file.
		/// </summary>
		/// <param name="end">
		///    A <see cref="long" /> value containing the seek position
		///    at which the media data ends and the tags begin.
		/// </param>
		/// <param name="propertiesStyle">
		///    A <see cref="ReadStyle" /> value specifying at what level
		///    of accuracy to read the media properties, or <see
		///    cref="ReadStyle.None" /> to ignore the properties.
		/// </param>
		protected override void ReadEnd (long end,
		                                 ReadStyle propertiesStyle)
		{
			// Make sure we have ID3v1 and ID3v2 tags.
			GetTag (TagTypes.Id3v1, true);
			GetTag (TagTypes.Id3v2, true);
		}
		
		/// <summary>
		///    Reads the audio properties from the file represented by
		///    the current instance.
		/// </summary>
		/// <param name="start">
		///    A <see cref="long" /> value containing the seek position
		///    at which the tags end and the media data begins.
		/// </param>
		/// <param name="end">
		///    A <see cref="long" /> value containing the seek position
		///    at which the media data ends and the tags begin.
		/// </param>
		/// <param name="propertiesStyle">
		///    A <see cref="ReadStyle" /> value specifying at what level
		///    of accuracy to read the media properties, or <see
		///    cref="ReadStyle.None" /> to ignore the properties.
		/// </param>
		/// <returns>
		///    A <see cref="TagLib.Properties" /> object describing the
		///    media properties of the file represented by the current
		///    instance.
		/// </returns>
		protected override Properties ReadProperties (long start,
		                                              long end,
		                                              ReadStyle propertiesStyle)
		{
			first_header.SetStreamLength (end - start);
			return new Properties (TimeSpan.Zero, first_header);
		}
		
		#endregion
	}
}