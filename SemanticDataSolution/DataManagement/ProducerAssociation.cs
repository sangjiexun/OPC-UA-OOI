﻿
using System;
using System.Collections.Generic;
using System.Linq;
using UAOOI.SemanticData.DataManagement.Configuration;
using UAOOI.SemanticData.DataManagement.DataRepository;
using UAOOI.SemanticData.DataManagement.MessageHandling;

namespace UAOOI.SemanticData.DataManagement
{

  /// <summary>
  /// Class ProducerAssociation - implements the association for the producer side.
  /// </summary>
  internal class ProducerAssociation : Association
  {

    #region creator
    /// <summary>
    /// Initializes a new instance of the <see cref="ProducerAssociation"/> class.
    /// </summary>
    /// <param name="data">The semantic data description.</param>
    /// <param name="aliasName">Name of the alias - .</param>
    /// <param name="dataSetConfiguration">The data set configuration.</param>
    /// <param name="bindingFactory">The binding factory.</param>
    /// <param name="encodingFactory">The encoding factory.</param>
    internal ProducerAssociation(ISemanticData data, string aliasName, DataSetConfiguration dataSetConfiguration, IBindingFactory bindingFactory, IEncodingFactory encodingFactory)
      : base(data, aliasName)
    {
      m_ProcessDataBindings =
        dataSetConfiguration.DataSet.Select<DataMemberConfiguration, IProducerBinding>
        ((x) =>
        {
          IProducerBinding _ret = x.GetProducerBinding4DataMember(dataSetConfiguration.RepositoryGroup, bindingFactory, encodingFactory);
          _ret.PropertyChanged += ProcessDataBindings_CollectionChanged;
          return _ret;
        }).ToArray<IProducerBinding>();
    }
    #endregion

    #region public API
    /// <summary>
    /// Adds the message writer.
    /// </summary>
    /// <param name="messageWriter">The message writer.</param>
    /// <exception cref="System.ArgumentNullException">messageReader</exception>
    public void AddMessageWriter(IMessageWriter messageWriter)
    {
      if (messageWriter == null)
        throw new ArgumentNullException("messageReader");
      if (!m_MessageWriter.Exists(x => x.Equals(messageWriter)))
        m_MessageWriter.Add(messageWriter);
    }
    /// <summary>
    /// Removes the message writer.
    /// </summary>
    /// <param name="messageWriter">The message writer.</param>
    /// <exception cref="System.ArgumentNullException">messageReader</exception>
    public void RemoveMessageWriter(IMessageWriter messageWriter)
    {
      if (messageWriter == null)
        throw new ArgumentNullException("messageReader");
      if (m_MessageWriter.Exists(x => x.Equals(messageWriter)))
        m_MessageWriter.Add(messageWriter);
    }
    #endregion

    #region private
    private object mLockObject = new object();
    bool m_Busy = false;
    protected override void InitializeCommunication()
    {
      //Do nothing;
    }
    protected override void OnEnabling()
    {
      foreach (IProducerBinding _pbx in m_ProcessDataBindings)
        _pbx.OnEnabling();
    }
    protected override void OnDisabling()
    {
      foreach (IProducerBinding _pbx in m_ProcessDataBindings)
        _pbx.OnDisabling();
    }
    protected internal override void AddMessageHandler(IMessageHandler messageHandler)
    {
      AddMessageWriter(messageHandler as IMessageWriter);
    }
    private List<IMessageWriter> m_MessageWriter = new List<IMessageWriter>();
    private IProducerBinding[] m_ProcessDataBindings;
    private void ProcessDataBindings_CollectionChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (m_Busy)
        return;
      m_Busy = true;
      foreach (IMessageWriter _mwx in m_MessageWriter)
        lock (mLockObject)
          _mwx.Send(x => m_ProcessDataBindings[x], m_ProcessDataBindings.Length, UInt64.MaxValue, DataDescriptor);
      m_Busy = false;
    }
    #endregion

  }

}
