
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMT.Models
{
    public class AmazonS3
    {
        public static void SubirArchivo(Stream stream, string fileName, string folder = "")
        {
            try
            {
                string awsAccessKeyID = ConfigurationManager.AppSettings["AWSAccessKey"];
                string awsSecretAccessKey = ConfigurationManager.AppSettings["AWSSecretKey"];
                string bucketName = ConfigurationManager.AppSettings["AWSBuket"];

                TransferUtility fileTransferUtility = new TransferUtility(awsAccessKeyID, awsSecretAccessKey, Amazon.RegionEndpoint.USEast1);
                fileTransferUtility.Upload(stream, bucketName + folder, fileName);
            }
            catch (AmazonS3Exception)
            {
                throw new Exception("Error de S3.");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static Stream DescargarArchivo(string fileName, string folder = "")
        {
            try
            {
                string awsAccessKeyID = ConfigurationManager.AppSettings["AWSAccessKey"];
                string awsSecretAccessKey = ConfigurationManager.AppSettings["AWSSecretKey"];
                string bucketName = ConfigurationManager.AppSettings["AWSBuket"];

                IAmazonS3 client;
                using (client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1))
                {
                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = bucketName + folder,
                        Key = fileName
                    };


                    GetObjectResponse response = client.GetObject(request);
                    
                    return response.ResponseStream;
                    
                }

            }
            catch (AmazonS3Exception e)
            {
                throw new Exception("Error de S3.");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void EliminarArchivo(string fileName, string folder = "")
        {
            try
            {
                string awsAccessKeyID = ConfigurationManager.AppSettings["AWSAccessKey"];
                string awsSecretAccessKey = ConfigurationManager.AppSettings["AWSSecretKey"];
                string bucketName = ConfigurationManager.AppSettings["AWSBuket"];
                string systemName = ConfigurationManager.AppSettings["AWSRegion"];

                AmazonS3Client client = new AmazonS3Client(awsAccessKeyID, awsSecretAccessKey, Amazon.RegionEndpoint.GetBySystemName(systemName));
                client.DeleteObject(bucketName + folder, fileName);
            }
            catch (AmazonS3Exception)
            {
                throw new Exception("Error de S3.");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
    }
}
