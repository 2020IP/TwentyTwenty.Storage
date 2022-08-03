using System;
// using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace TwentyTwenty.Storage.Amazon
{
    public class AmazonFileUploader
    {
        private const int PART_SIZE = 6 * 1024 * 1024;
        private const int READ_BUFFER_SIZE = 20000;
        private readonly IAmazonS3 _s3Client;

        public AmazonFileUploader(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task UploadFileAsync(string bucketName, string key, Stream source, string contentType, S3CannedACL cannedACL,
            ServerSideEncryptionMethod encryption, bool autoCloseStream, Action<UploadEvent> callback)
        {
            //this._logger.LogInformation($"Start uploading to {objectKey}");
            var initateResponse = await _s3Client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = key,
                ContentType = contentType,
                CannedACL = cannedACL,
                ServerSideEncryptionMethod = encryption,
            });
            //this._logger.LogInformation($"Initiated multi part upload with id {initateResponse.UploadId}");
            try
            {
                var partETags = new List<PartETag>();
                // var readBuffer = ArrayPool<byte>.Shared.Rent(READ_BUFFER_SIZE);
                // var partBuffer = ArrayPool<byte>.Shared.Rent(PART_SIZE + (READ_BUFFER_SIZE * 3));
                var readBuffer = new byte[READ_BUFFER_SIZE];
                var partBuffer = new byte[PART_SIZE + (READ_BUFFER_SIZE * 3)];

                var callbackEvent = new UploadEvent();
                var nextUploadBuffer = new MemoryStream(partBuffer);
                try
                {
                    int partNumber = 1;
                    int readCount;
                    while ((readCount = await source.ReadAsync(readBuffer, 0, readBuffer.Length)) != 0)
                    {
                        callbackEvent.UploadBytes += readCount;
                        callback?.Invoke(callbackEvent);

                        await nextUploadBuffer.WriteAsync(readBuffer, 0, readCount);

                        if(PART_SIZE < nextUploadBuffer.Position)
                        {
                            var isLastPart = readCount == READ_BUFFER_SIZE;
                            var partSize = nextUploadBuffer.Position;
                            nextUploadBuffer.Position = 0;
                            var partResponse = await _s3Client.UploadPartAsync(new UploadPartRequest
                            {
                                BucketName = bucketName,
                                Key = key,
                                UploadId = initateResponse.UploadId,
                                InputStream = nextUploadBuffer,
                                PartSize = partSize,
                                PartNumber = partNumber,  
                                IsLastPart = isLastPart
                            });
                            // this._logger.LogInformation($"Uploaded part {partNumber}. (Last part = {isLastPart}, Part size = {partSize}, Upload Id: {initateResponse.UploadId}");

                            partETags.Add(new PartETag { PartNumber = partResponse.PartNumber, ETag = partResponse.ETag });
                            partNumber++;
                            nextUploadBuffer = new MemoryStream(partBuffer);

                            callbackEvent.UploadParts++;
                            callback?.Invoke(callbackEvent);
                        }
                    }

                    if(nextUploadBuffer.Position != 0)
                    {
                        var partSize = nextUploadBuffer.Position;
                        nextUploadBuffer.Position = 0;
                        var partResponse = await _s3Client.UploadPartAsync(new UploadPartRequest
                        {
                            BucketName = bucketName,
                            Key = key,
                            UploadId = initateResponse.UploadId,
                            InputStream = nextUploadBuffer,
                            PartSize = partSize,
                            PartNumber = partNumber,
                            IsLastPart = true
                        });
                        // this._logger.LogInformation($"Uploaded final part. (Part size = {partSize}, Upload Id: {initateResponse.UploadId})");
                        partETags.Add(new PartETag { PartNumber = partResponse.PartNumber, ETag = partResponse.ETag });

                        callbackEvent.UploadParts++;
                        callback?.Invoke(callbackEvent);
                    }
                }
                finally
                {
                    // ArrayPool<byte>.Shared.Return(partBuffer);
                    // ArrayPool<byte>.Shared.Return(readBuffer);

                    if (autoCloseStream)
                    {
                        source.Close();
                    }
                }

                await _s3Client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    UploadId = initateResponse.UploadId,
                    PartETags = partETags
                });
                // this._logger.LogInformation($"Completed multi part upload. (Part count: {partETags.Count}, Upload Id: {initateResponse.UploadId})");
            }
            catch
            {
                await _s3Client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    UploadId = initateResponse.UploadId
                });
                // this._logger.LogError($"Error uploading to S3 with error: {e.Message}");

                throw;
            }
        }
    }

    public class UploadEvent
    {
        public long UploadBytes { get; set; }
        public int UploadParts { get; set; }
    }
}