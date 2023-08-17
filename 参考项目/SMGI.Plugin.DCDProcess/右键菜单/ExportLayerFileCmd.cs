using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;

namespace SMGI.Plugin.DCDProcess
{
    public class ExportLayerFileCmd : SMGI.Common.SMGIContextMenu
    {
        public ExportLayerFileCmd()
        {
            m_caption = "另存为图层文件";
        }

        public override bool Enabled
        {
            get
            {
                return this.CurrentContextItem is ILayer;
            }
        }
        public override void OnClick()
        {
            ILayer player = this.CurrentContextItem as ILayer;
            if (player == null)
            {
                return;
            }

            if (player is IFeatureLayer)
            {
                SaveFileDialog pSaveFileDialog = new SaveFileDialog();
                pSaveFileDialog.Title = "保存图层";
                pSaveFileDialog.Filter = "图层文件(*.lyr)|*.lyr";
                if (pSaveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    IFeatureLayer fl = player as IFeatureLayer;
                    ILayerFile layerFile = new LayerFileClass();
                    layerFile.New(pSaveFileDialog.FileName);
                    layerFile.ReplaceContents(player);
                    layerFile.Save();
                }
            }
        }
    }
}
