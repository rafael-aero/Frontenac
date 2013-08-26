﻿using System;
using System.IO;
using Grave.Esent.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Grave.Esent
{
    public class EsentContext
    {
        JET_DBID _dbid;
        protected readonly IContentSerializer ContentSerializer;

        public string DatabaseName { get; protected set; }
        public EsentVertexTable VertexTable { get; private set; }
        public EsentEdgesTable EdgesTable { get; private set; }
        public Session Session { get; private set; }

        public EsentContext(Session session, string databaseName, IContentSerializer contentSerializer)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            if(contentSerializer == null)
                throw new ArgumentNullException("contentSerializer");

            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("databaseName");

            Session = session;
            DatabaseName = CleanDatabaseName(databaseName);
            ContentSerializer = contentSerializer;

            VertexTable = new EsentVertexTable(session, contentSerializer);
            EdgesTable = new EsentEdgesTable(session, contentSerializer);

            Init();
        }

        #region IDisposable
        bool _disposed;

        ~EsentContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                CloseDatabase();
            }

            _disposed = true;
        }

        #endregion

        static string CleanDatabaseName(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("databaseName");

            databaseName = databaseName.Trim();

            if (databaseName.EndsWith("."))
                databaseName = databaseName.Remove(databaseName.Length - 1);

            if (!databaseName.ToLower().EndsWith(".db"))
                databaseName = string.Concat(databaseName, ".db");

            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("databaseName");

            if (databaseName == ".db")
                throw new ArgumentException("databaseName");

            return databaseName;
        }

        public static Instance CreateInstance(string instanceName, string logsDirectory, string tempDirectory, string systemDirectory)
        {
            var instance = new Instance(instanceName);
            instance.Parameters.CircularLog = true;
            instance.Parameters.Recovery = true;
            instance.Parameters.LogBuffers = 8 * 1024;
            instance.Parameters.LogFileSize = 16 * 1024;
            instance.Parameters.SystemDirectory = systemDirectory;
            instance.Parameters.TempDirectory = tempDirectory;
            instance.Parameters.LogFileDirectory = logsDirectory;
            instance.Parameters.CreatePathIfNotExist = true;
            SystemParameters.CacheSizeMin = 16 * 1024;
            instance.Init();
            return instance;
        }

        public EsentVertexTable GetVerticesCursor()
        {
            var cursor = new EsentVertexTable(Session, ContentSerializer);
            cursor.Open(_dbid);
            return cursor;
        }

        public EsentEdgesTable GetEdgesCursor()
        {
            var cursor = new EsentEdgesTable(Session, ContentSerializer);
            cursor.Open(_dbid);
            return cursor;
        }

        protected virtual JET_DBID OpenDatabase(OpenDatabaseGrbit openFlags)
        {
            JET_DBID dbid;
            Api.JetAttachDatabase(Session, DatabaseName, AttachDatabaseGrbit.DeleteCorruptIndexes);
            Api.JetOpenDatabase(Session, DatabaseName, null, out dbid, OpenDatabaseGrbit.None);
            VertexTable.Open(dbid);
            EdgesTable.Open(dbid);
            return dbid;
        }

        internal protected void Init()
        {
            _dbid = OpenDatabase();
        }

        protected virtual JET_DBID OpenDatabase()
        {
            return OpenDatabase(OpenDatabaseGrbit.None);
        }
        
        protected virtual void CloseDatabase()
        {
            VertexTable.Close();
            EdgesTable.Close();
        }
    }
}