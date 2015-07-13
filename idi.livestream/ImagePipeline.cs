using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace idi.livestream
{
    public class ImagePipeline : UtilMPipeline
    {
        // event handler
        public delegate void ImagePipelineHandler(object sender, UInt16[] depthImage, byte[] rgbImage);
        public event ImagePipelineHandler handler;

        private bool finished;

        // image variables
        public uint dWitdh, dHeight;
        public UInt16[] depthImageUInt16;
        public uint rgbWitdh, rgbHeight;
        public byte[] rgbImageByte;

        public ImagePipeline() : base()
        {
            finished = false;
            EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH);
            EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_RGB24);
        }

        public bool IsReady()
        {
            return depthImageUInt16 != null && rgbImageByte != null;
        }

        public unsafe override bool OnNewFrame()
        {
            /******************************RGB********************************/
            PXCMImage rgbPXCMImage = QueryImage(PXCMImage.ImageType.IMAGE_TYPE_COLOR);
            PXCMImage.ImageData rgbData;
            rgbPXCMImage.AcquireAccess(PXCMImage.Access.ACCESS_READ, out rgbData);
            if (rgbImageByte == null)
            {
                rgbWitdh = rgbPXCMImage.info.width;
                rgbHeight = rgbPXCMImage.info.height;
                rgbImageByte = new byte[rgbWitdh*rgbHeight*3];
            }

            byte* colorPointer = (byte*)(rgbData.buffer.planes[0]);
            for (int i = 0; i < rgbWitdh * rgbHeight * 3; i++)
            {
                rgbImageByte[i] = colorPointer[i];
            }

            /*********************** Depth && UV *********************************/
            PXCMImage pXCMIimage = QueryImage(PXCMImage.ImageType.IMAGE_TYPE_DEPTH);
            PXCMImage.ImageData data;
            pXCMIimage.AcquireAccess(PXCMImage.Access.ACCESS_READ, out data);
            if (depthImageUInt16 == null)
            {
                dWitdh = pXCMIimage.info.width;
                dHeight = pXCMIimage.info.height;
                depthImageUInt16 = new UInt16[pXCMIimage.info.width * pXCMIimage.info.height];
            }

            UInt16* depthPointer = (UInt16*)(data.buffer.planes[0]);
            for (uint i = 0; i < dWitdh * dHeight; i++)
            {
                depthImageUInt16[i] = depthPointer[i];
            }

            pXCMIimage.ReleaseAccess(ref data);
            rgbPXCMImage.ReleaseAccess(ref rgbData);
            this.ReleaseFrame();

            if (handler != null)
            {
                handler(this, depthImageUInt16, rgbImageByte);
            }
            return !finished;
        }

        public void Finish()
        {
            finished = true;
        }

        public override bool OnDisconnect()
        {
            Finish();
            return false;
        }

        public bool IsFinished()
        {
            return finished;
        }

        public void StartWork()
        {
            finished = false;
            this.LoopFrames();
        }
    }
}
