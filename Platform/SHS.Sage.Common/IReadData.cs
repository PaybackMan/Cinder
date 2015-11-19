using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public interface IReadData : IDisposable
    {
        //
        // Summary:
        //     Gets a value indicating the depth of nesting for the current row.
        //
        // Returns:
        //     The level of nesting.
        int Depth { get; }
        //
        // Summary:
        //     Gets a value indicating whether the data reader is closed.
        //
        // Returns:
        //     true if the data reader is closed; otherwise, false.
        bool IsClosed { get; }
        //
        // Summary:
        //     Gets the number of rows changed, inserted, or deleted by execution of the SQL
        //     statement.
        //
        // Returns:
        //     The number of rows changed, inserted, or deleted; 0 if no rows were affected
        //     or the statement failed; and -1 for SELECT statements.
        int RecordsAffected { get; }
        //
        // Summary:
        //     Advances the data reader to the next result, when reading the results of batch
        //     SQL statements.
        //
        // Returns:
        //     true if there are more rows; otherwise, false.
        bool NextResult();
        //
        // Summary:
        //     Advances the System.Data.IDataReader to the next record.
        //
        // Returns:
        //     true if there are more rows; otherwise, false.
        bool Read();
        //
        // Summary:
        //     Gets the column with the specified name.
        //
        // Parameters:
        //   name:
        //     The name of the column to find.
        //
        // Returns:
        //     The column with the specified name as an System.Object.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     No column with the specified name was found.
        object this[string name] { get; }
        //
        // Summary:
        //     Gets the column located at the specified index.
        //
        // Parameters:
        //   i:
        //     The zero-based index of the column to get.
        //
        // Returns:
        //     The column located at the specified index as an System.Object.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        object this[int i] { get; }

        //
        // Summary:
        //     Gets the number of columns in the current row.
        //
        // Returns:
        //     When not positioned in a valid recordset, 0; otherwise, the number of columns
        //     in the current record. The default is -1.
        int FieldCount { get; }

        //
        // Summary:
        //     Gets the value of the specified column as a Boolean.
        //
        // Parameters:
        //   i:
        //     The zero-based column ordinal.
        //
        // Returns:
        //     The value of the column.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        bool GetBoolean(int i);
        //
        // Summary:
        //     Gets the 8-bit unsigned integer value of the specified column.
        //
        // Parameters:
        //   i:
        //     The zero-based column ordinal.
        //
        // Returns:
        //     The 8-bit unsigned integer value of the specified column.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        byte GetByte(int i);
        //
        // Summary:
        //     Reads a stream of bytes from the specified column offset into the buffer as an
        //     array, starting at the given buffer offset.
        //
        // Parameters:
        //   i:
        //     The zero-based column ordinal.
        //
        //   fieldOffset:
        //     The index within the field from which to start the read operation.
        //
        //   buffer:
        //     The buffer into which to read the stream of bytes.
        //
        //   bufferoffset:
        //     The index for buffer to start the read operation.
        //
        //   length:
        //     The number of bytes to read.
        //
        // Returns:
        //     The actual number of bytes read.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length);
        //
        // Summary:
        //     Gets the character value of the specified column.
        //
        // Parameters:
        //   i:
        //     The zero-based column ordinal.
        //
        // Returns:
        //     The character value of the specified column.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        char GetChar(int i);
        //
        // Summary:
        //     Reads a stream of characters from the specified column offset into the buffer
        //     as an array, starting at the given buffer offset.
        //
        // Parameters:
        //   i:
        //     The zero-based column ordinal.
        //
        //   fieldoffset:
        //     The index within the row from which to start the read operation.
        //
        //   buffer:
        //     The buffer into which to read the stream of bytes.
        //
        //   bufferoffset:
        //     The index for buffer to start the read operation.
        //
        //   length:
        //     The number of bytes to read.
        //
        // Returns:
        //     The actual number of characters read.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length);
        //
        // Summary:
        //     Gets the data type information for the specified field.
        //
        // Parameters:
        //   i:
        //     The index of the field to find.
        //
        // Returns:
        //     The data type information for the specified field.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        string GetDataTypeName(int i);
        //
        // Summary:
        //     Gets the date and time data value of the specified field.
        //
        // Parameters:
        //   i:
        //     The index of the field to find.
        //
        // Returns:
        //     The date and time data value of the specified field.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        DateTime GetDateTime(int i);
        //
        // Summary:
        //     Gets the fixed-position numeric value of the specified field.
        //
        // Parameters:
        //   i:
        //     The index of the field to find.
        //
        // Returns:
        //     The fixed-position numeric value of the specified field.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        decimal GetDecimal(int i);
        //
        // Summary:
        //     Gets the double-precision floating point number of the specified field.
        //
        // Parameters:
        //   i:
        //     The index of the field to find.
        //
        // Returns:
        //     The double-precision floating point number of the specified field.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        double GetDouble(int i);
        //
        // Summary:
        //     Gets the System.Type information corresponding to the type of System.Object that
        //     would be returned from System.Data.IDataRecord.GetValue(System.Int32).
        //
        // Parameters:
        //   i:
        //     The index of the field to find.
        //
        // Returns:
        //     The System.Type information corresponding to the type of System.Object that would
        //     be returned from System.Data.IDataRecord.GetValue(System.Int32).
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        Type GetFieldType(int i);
        //
        // Summary:
        //     Gets the single-precision floating point number of the specified field.
        //
        // Parameters:
        //   i:
        //     The index of the field to find.
        //
        // Returns:
        //     The single-precision floating point number of the specified field.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        float GetFloat(int i);
        //
        // Summary:
        //     Returns the GUID value of the specified field.
        //
        // Parameters:
        //   i:
        //     The index of the field to find.
        //
        // Returns:
        //     The GUID value of the specified field.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        Guid GetGuid(int i);
        //
        // Summary:
        //     Gets the 16-bit signed integer value of the specified field.
        //
        // Parameters:
        //   i:
        //     The index of the field to find.
        //
        // Returns:
        //     The 16-bit signed integer value of the specified field.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        short GetInt16(int i);
        //
        // Summary:
        //     Gets the 32-bit signed integer value of the specified field.
        //
        // Parameters:
        //   i:
        //     The index of the field to find.
        //
        // Returns:
        //     The 32-bit signed integer value of the specified field.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        int GetInt32(int i);
        //
        // Summary:
        //     Gets the 64-bit signed integer value of the specified field.
        //
        // Parameters:
        //   i:
        //     The index of the field to find.
        //
        // Returns:
        //     The 64-bit signed integer value of the specified field.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        long GetInt64(int i);
        //
        // Summary:
        //     Gets the name for the field to find.
        //
        // Parameters:
        //   i:
        //     The index of the field to find.
        //
        // Returns:
        //     The name of the field or the empty string (""), if there is no value to return.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        string GetName(int i);
        //
        // Summary:
        //     Return the index of the named field.
        //
        // Parameters:
        //   name:
        //     The name of the field to find.
        //
        // Returns:
        //     The index of the named field.
        int GetOrdinal(string name);
        //
        // Summary:
        //     Gets the string value of the specified field.
        //
        // Parameters:
        //   i:
        //     The index of the field to find.
        //
        // Returns:
        //     The string value of the specified field.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        string GetString(int i);
        //
        // Summary:
        //     Return the value of the specified field.
        //
        // Parameters:
        //   i:
        //     The index of the field to find.
        //
        // Returns:
        //     The System.Object which will contain the field value upon return.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        object GetValue(int i);
        //
        // Summary:
        //     Populates an array of objects with the column values of the current record.
        //
        // Parameters:
        //   values:
        //     An array of System.Object to copy the attribute fields into.
        //
        // Returns:
        //     The number of instances of System.Object in the array.
        int GetValues(object[] values);
        //
        // Summary:
        //     Return whether the specified field is set to null.
        //
        // Parameters:
        //   i:
        //     The index of the field to find.
        //
        // Returns:
        //     true if the specified field is set to null; otherwise, false.
        //
        // Exceptions:
        //   T:System.IndexOutOfRangeException:
        //     The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.
        bool IsDBNull(int i);
    }
}
