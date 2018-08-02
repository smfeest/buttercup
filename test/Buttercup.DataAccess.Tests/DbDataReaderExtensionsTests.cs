using System;
using System.Data.Common;
using Moq;
using Xunit;

namespace Buttercup.DataAccess
{
    public class DbDataReaderExtensionsTests
    {
        #region GetDateTime

        [Theory]
        [InlineData(DateTimeKind.Local)]
        [InlineData(DateTimeKind.Unspecified)]
        [InlineData(DateTimeKind.Utc)]
        public void GetDateTimeReturnsValueWithDateTimeKind(DateTimeKind kind)
        {
            var mockDbDataReader = new Mock<DbDataReader>();

            mockDbDataReader
                .Setup(x => x.GetOrdinal("alpha"))
                .Returns(10);
            mockDbDataReader
                .Setup(x => x.GetDateTime(10))
                .Returns(new DateTime(2000, 1, 2, 3, 4, 5, 6, DateTimeKind.Unspecified));

            Assert.Equal(
                new DateTime(2000, 1, 2, 3, 4, 5, 6, kind),
                mockDbDataReader.Object.GetDateTime("alpha", kind));
        }

        #endregion

        #region GetInt32

        [Fact]
        public void GetInt32ReturnsValue()
        {
            var mockDbDataReader = new Mock<DbDataReader>();

            mockDbDataReader
                .Setup(x => x.GetOrdinal("alpha"))
                .Returns(10);
            mockDbDataReader
                .Setup(x => x.GetInt32(10))
                .Returns(-23);

            Assert.Equal(-23, mockDbDataReader.Object.GetInt32("alpha"));

            mockDbDataReader.Verify(x => x.IsDBNull(10), Times.Never);
        }

        #endregion

        #region GetInt64

        [Fact]
        public void GetInt64ReturnsValue()
        {
            var mockDbDataReader = new Mock<DbDataReader>();

            mockDbDataReader
                .Setup(x => x.GetOrdinal("alpha"))
                .Returns(10);
            mockDbDataReader
                .Setup(x => x.GetInt64(10))
                .Returns(343);

            Assert.Equal(343, mockDbDataReader.Object.GetInt64("alpha"));

            mockDbDataReader.Verify(x => x.IsDBNull(10), Times.Never);
        }

        #endregion

        #region GetNullableInt32

        [Fact]
        public void GetNullableInt32ReturnsValueWhenNotDbNull()
        {
            var mockDbDataReader = new Mock<DbDataReader>();

            mockDbDataReader
                .Setup(x => x.GetOrdinal("alpha"))
                .Returns(10);
            mockDbDataReader
                .Setup(x => x.IsDBNull(10))
                .Returns(false);
            mockDbDataReader
                .Setup(x => x.GetInt32(10))
                .Returns(5);

            Assert.Equal(5, mockDbDataReader.Object.GetNullableInt32("alpha"));
        }

        [Fact]
        public void GetNullableInt32ReturnsNullWhenValueIsDbNull()
        {
            var mockDbDataReader = new Mock<DbDataReader>();

            mockDbDataReader
                .Setup(x => x.GetOrdinal("alpha"))
                .Returns(10);
            mockDbDataReader
                .Setup(x => x.IsDBNull(10))
                .Returns(true);

            Assert.Null(mockDbDataReader.Object.GetNullableInt32("alpha"));

            mockDbDataReader.Verify(x => x.GetInt32(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region GetNullableInt64

        [Fact]
        public void GetNullableInt64ReturnsValueWhenNotDbNull()
        {
            var mockDbDataReader = new Mock<DbDataReader>();

            mockDbDataReader
                .Setup(x => x.GetOrdinal("alpha"))
                .Returns(10);
            mockDbDataReader
                .Setup(x => x.IsDBNull(10))
                .Returns(false);
            mockDbDataReader
                .Setup(x => x.GetInt64(10))
                .Returns(long.MinValue);

            Assert.Equal(long.MinValue, mockDbDataReader.Object.GetNullableInt64("alpha"));
        }

        [Fact]
        public void GetNullableInt64ReturnsNullWhenValueIsDbNull()
        {
            var mockDbDataReader = new Mock<DbDataReader>();

            mockDbDataReader
                .Setup(x => x.GetOrdinal("alpha"))
                .Returns(10);
            mockDbDataReader
                .Setup(x => x.IsDBNull(10))
                .Returns(true);

            Assert.Null(mockDbDataReader.Object.GetNullableInt64("alpha"));

            mockDbDataReader.Verify(x => x.GetInt64(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region GetString

        [Fact]
        public void GetStringReturnsValueWhenNotDbNull()
        {
            var mockDbDataReader = new Mock<DbDataReader>();

            mockDbDataReader
                .Setup(x => x.GetOrdinal("alpha"))
                .Returns(10);
            mockDbDataReader
                .Setup(x => x.IsDBNull(10))
                .Returns(false);
            mockDbDataReader
                .Setup(x => x.GetString(10))
                .Returns("beta");

            Assert.Equal("beta", mockDbDataReader.Object.GetString("alpha"));
        }

        [Fact]
        public void GetStringReturnsNullWhenValueIsDbNull()
        {
            var mockDbDataReader = new Mock<DbDataReader>();

            mockDbDataReader
                .Setup(x => x.GetOrdinal("alpha"))
                .Returns(10);
            mockDbDataReader
                .Setup(x => x.IsDBNull(10))
                .Returns(true);

            Assert.Null(mockDbDataReader.Object.GetString("alpha"));

            mockDbDataReader.Verify(x => x.GetString(It.IsAny<int>()), Times.Never);
        }

        #endregion
    }
}
