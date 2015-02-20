#!/usr/bin/env python

"""
Copyright (c) 2006-2012 sqlmap developers (http://sqlmap.org/)
See the file 'doc/COPYING' for copying permission
"""

try:
    import _mssql
    import pymssql
except ImportError, _:
    pass

from lib.core.convert import utf8encode
from lib.core.data import conf
from lib.core.data import logger
from lib.core.exception import sqlmapConnectionException
from plugins.generic.connector import Connector as GenericConnector

class Connector(GenericConnector):
    """
    Homepage: http://pymssql.sourceforge.net/
    User guide: http://pymssql.sourceforge.net/examples_pymssql.php
    API: http://pymssql.sourceforge.net/ref_pymssql.php
    Debian package: python-pymssql
    License: LGPL

    Possible connectors: http://wiki.python.org/moin/SQL%20Server

    Important note: pymssql library on your system MUST be version 1.0.2
    to work, get it from http://sourceforge.net/projects/pymssql/files/pymssql/1.0.2/
    """

    def __init__(self):
        GenericConnector.__init__(self)

    def connect(self):
        self.initConnection()

        try:
            self.connector = pymssql.connect(host="%s:%d" % (self.hostname, self.port), user=self.user, password=self.password, database=self.db, login_timeout=conf.timeout, timeout=conf.timeout)
        except pymssql.OperationalError, msg:
            raise sqlmapConnectionException, msg

        self.setCursor()
        self.connected()

    def fetchall(self):
        try:
            return self.cursor.fetchall()
        except (pymssql.ProgrammingError, pymssql.OperationalError, _mssql.MssqlDatabaseException), msg:
            logger.warn("(remote) %s" % msg)
            return None

    def execute(self, query):
        try:
            self.cursor.execute(utf8encode(query))
        except (pymssql.OperationalError, pymssql.ProgrammingError), msg:
            logger.warn("(remote) %s" % msg)
        except pymssql.InternalError, msg:
            raise sqlmapConnectionException, msg

    def select(self, query):
        self.execute(query)
        value = self.fetchall()

        try:
            self.connector.commit()
        except pymssql.OperationalError:
            pass

        return value
