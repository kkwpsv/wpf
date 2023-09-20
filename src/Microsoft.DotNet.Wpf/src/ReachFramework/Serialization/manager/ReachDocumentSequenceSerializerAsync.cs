// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Security;
using System.ComponentModel.Design.Serialization;
using System.Windows.Xps.Packaging;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Markup;

namespace System.Windows.Xps.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    internal class DocumentSequenceSerializerAsync :
                   ReachSerializerAsync
    {
        /// <summary>
        /// 
        /// </summary>
        public
        DocumentSequenceSerializerAsync(
            PackageSerializationManager   manager
            ):
        base(manager)
        {
            
        }

        public
        override
        void
        AsyncOperation(
            ReachSerializerContext context
            )
        {
            if(context == null)
            {

            }
           
            switch (context.Action) 
            {
                case SerializerAction.endPersistObjectData:
                {
                    EndPersistObjectData();
                    break;
                }
                
                default:
                {
                    base.AsyncOperation(context);
                    break;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        internal
        override
        void
        PersistObjectData(
            SerializableObjectContext   serializableObjectContext
            )
        {
            String xmlnsForType = SerializationManager.GetXmlNSForType(typeof(FixedDocumentSequence));

            if( SerializationManager is XpsSerializationManager)
            {
               (SerializationManager as XpsSerializationManager).RegisterDocumentSequenceStart();
            }

            if(xmlnsForType == null)
            {
                XmlWriter.WriteStartElement(serializableObjectContext.Name);
            }
            else
            {
                XmlWriter.WriteStartElement(serializableObjectContext.Name,
                                            xmlnsForType);
            }

            {
                ReachSerializerContext context = new ReachSerializerContext(this,
                                                                            SerializerAction.endPersistObjectData);

                ((IXpsSerializationManagerAsync)SerializationManager).OperationStack.Push(context);

                if(serializableObjectContext.IsComplexValue)
                {
                    XpsSerializationPrintTicketRequiredEventArgs e = 
                    new XpsSerializationPrintTicketRequiredEventArgs(PrintTicketLevel.FixedDocumentSequencePrintTicket,
                                                                     0);

                    ((IXpsSerializationManager)SerializationManager).OnXPSSerializationPrintTicketRequired(e);

                    //
                    // Serialize the data for the PrintTicket
                    //
                    if(e.Modified)
                    {
                        if(e.PrintTicket != null)
                        {
                            PrintTicketSerializerAsync serializer = new PrintTicketSerializerAsync(SerializationManager);
                            serializer.SerializeObject(e.PrintTicket);
                        }
                    }

                    SerializeObjectCore(serializableObjectContext);
                }
                else
                {

                }
            }
        }

        internal
        override
        void
        EndPersistObjectData(
            )
        {
            XmlWriter.WriteEndElement();
            XmlWriter = null;

            //
            // Signal to any registered callers that the Sequence has been serialized
            //
            XpsSerializationProgressChangedEventArgs progressEvent = 
            new XpsSerializationProgressChangedEventArgs(XpsWritingProgressChangeLevel.FixedDocumentSequenceWritingProgress,
                                                         0,
                                                         0,
                                                         null);

            if( SerializationManager is XpsSerializationManager)
            {
               (SerializationManager as XpsSerializationManager).RegisterDocumentSequenceEnd();
            }
            ((IXpsSerializationManager)SerializationManager).OnXPSSerializationProgressChanged(progressEvent);
        }

    
        /// <summary>
        /// 
        /// </summary>
        public
        override
        XmlWriter
        XmlWriter
        {
            get
            {
                if(base.XmlWriter == null)
                {
                    base.XmlWriter = SerializationManager.AcquireXmlWriter(typeof(FixedDocumentSequence));
                }

                return base.XmlWriter;
            }

            set
            {
                base.XmlWriter = null;
                SerializationManager.ReleaseXmlWriter(typeof(FixedDocumentSequence));
            }
        }

        /// <summary>
        ///
        /// </summary>
        internal
        override
        void
        WriteSerializedAttribute(
            SerializablePropertyContext serializablePropertyContext
            )
        {
            ArgumentNullException.ThrowIfNull(serializablePropertyContext);

            String attributeValue = String.Empty;

            attributeValue = GetValueOfAttributeAsString(serializablePropertyContext);

            if ( (attributeValue != null) && 
                 (attributeValue.Length > 0) )
            {
                //
                // Emit name="value" attribute
                //
                XmlWriter.WriteAttributeString(serializablePropertyContext.Name, attributeValue);
            }
        }

        internal
        String
        GetValueOfAttributeAsString(
            SerializablePropertyContext serializablePropertyContext
            )
        {
            ArgumentNullException.ThrowIfNull(serializablePropertyContext);

            String valueAsString                  = null;
            Object targetObjectContainingProperty = serializablePropertyContext.TargetObject;
            Object propertyValue                  = serializablePropertyContext.Value;

            if(propertyValue != null)
            {
                TypeConverter typeConverter = serializablePropertyContext.TypeConverter;

                valueAsString = typeConverter.ConvertToInvariantString(new XpsTokenContext(SerializationManager,
                                                                                             serializablePropertyContext),
                                                                       propertyValue);


                if (propertyValue is Type)
                {
                    int index = valueAsString.LastIndexOf('.');
                    valueAsString = string.Concat(
                        XpsSerializationManager.TypeOfString,
                        index > 0 ? valueAsString.AsSpan(index + 1) : valueAsString,
                        "}");
                }
            }
            else
            {
                valueAsString = XpsSerializationManager.NullString;
            }

            return valueAsString;
        }
    };
}
    

