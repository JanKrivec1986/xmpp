// Response.cs
//
//Ubiety XMPP Library Copyright (C) 2006 - 2009 Dieter Lunn
// 
// This library is free software; you can redistribute it and/or modify it under
// the terms of the GNU Lesser General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option)
// any later version.
// 
// This library is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
// 
// You should have received a copy of the GNU Lesser General Public License along
// with this library; if not, write to the Free Software Foundation, Inc., 59
// Temple Place, Suite 330, Boston, MA 02111-1307 USA

//
// Bdev.Net.Dns by Rob Philpott, Big Developments Ltd. Please send all bugs/enhancements to
// rob@bigdevelopments.co.uk  This file and the code contained within is freeware and may be
// distributed and edited without restriction.
// 

using System;
using System.Collections.Generic;

namespace ubiety.net.dns
{
	/// <summary>
	/// A Response is a logical representation of the byte data returned from a DNS query
	/// </summary>
	public class Response
	{
		// these are fields we're interested in from the message
		private readonly ReturnCode			_returnCode;
		private readonly bool				_authoritativeAnswer;
		private readonly bool				_recursionAvailable;
		private readonly bool				_truncated;
		private readonly Question[]			_questions;
		private readonly Answer[]			_answers;
		private readonly NameServer[]		_nameServers;
		private readonly AdditionalRecord[]	_additionalRecords;

		// these fields are readonly outside the assembly - use r/o properties
		///<summary>
		///</summary>
		public ReturnCode ReturnCode				{ get { return _returnCode;					}}
		///<summary>
		///</summary>
		public bool AuthoritativeAnswer				{ get { return _authoritativeAnswer;		}}
		///<summary>
		///</summary>
		public bool RecursionAvailable				{ get { return _recursionAvailable;			}}
		///<summary>
		///</summary>
		public bool MessageTruncated				{ get { return _truncated;					}}
		///<summary>
		///</summary>
		public Question[] Questions					{ get { return _questions;					}}
		///<summary>
		///</summary>
		public Answer[] Answers						{ get { return _answers;					}}
		///<summary>
		///</summary>
		public NameServer[] NameServers				{ get { return _nameServers;				}}
		///<summary>
		///</summary>
		public AdditionalRecord[] AdditionalRecords	{ get { return _additionalRecords;			}}

		/// <summary>
		/// Construct a Response object from the supplied byte array
		/// </summary>
		/// <param name="message">a byte array returned from a DNS server query</param>
		internal Response(byte[] message)
		{
			// the bit flags are in bytes 2 and 3
			var flags1 = message[2];
			var flags2 = message[3];

			// get return code from lowest 4 bits of byte 3
			var returnCode = flags2 & 15;
				
			// if its in the reserved section, set to other
			if (returnCode > 6) returnCode = 6;
			_returnCode = (ReturnCode)returnCode;

			// other bit flags
			_authoritativeAnswer = ((flags1 & 4) != 0);
			_recursionAvailable = ((flags2 & 128) != 0);
			_truncated = ((flags1 & 2) != 0);

			// create the arrays of response objects
			_questions = new Question[GetShort(message, 4)];
			_answers = new Answer[GetShort(message, 6)];
			_nameServers = new NameServer[GetShort(message, 8)];
			_additionalRecords = new AdditionalRecord[GetShort(message, 10)];

			// need a pointer to do this, position just after the header
			var pointer = new Pointer(message, 12);

			// and now populate them, they always follow this order
			for (var index = 0; index < _questions.Length; index++)
			{
				try
				{
					// try to build a quesion from the response
					_questions[index] = new Question(pointer);
				}
				catch (Exception ex)
				{
					// something grim has happened, we can't continue
					throw new InvalidResponseException(ex);
				}
			}
			for (var index = 0; index < _answers.Length; index++)
			{
				_answers[index] = new Answer(pointer);
			}
			for (var index = 0; index < _nameServers.Length; index++)
			{
				_nameServers[index] = new NameServer(pointer);
			}
			for (var index = 0; index < _additionalRecords.Length; index++)
			{
				_additionalRecords[index] = new AdditionalRecord(pointer);
			}
		}
	
		/// <summary>
		/// Convert 2 bytes to a short. It would have been nice to use BitConverter for this,
		/// it however reads the bytes in the wrong order (at least on Windows)
		/// </summary>
		/// <param name="message">byte array to look in</param>
		/// <param name="position">position to look at</param>
		/// <returns>short representation of the two bytes</returns>
		private static short GetShort(IList<byte> message, int position)
		{
			return (short)(message[position]<<8 | message[position+1]);
		}
	}
}
