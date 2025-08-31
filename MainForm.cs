using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace CoreBrush
{
    public partial class MainForm : Form
    {
        private PictureBox originalImageBox = null!;
        private PictureBox transformedImageBox = null!;
        private Label originalLabel = null!;
        private Label transformedLabel = null!;
        private MenuStrip menuStrip = null!;
        private OpenFileDialog openFileDialog = null!;
        private SaveFileDialog saveFileDialog = null!;
        private Bitmap? originalImage;
        private Bitmap? transformedImage;

        public MainForm()
        {
            InitializeComponent();
            InitializeForm();
            CreateMenus();
            CreatePictureBoxes();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }

        private void InitializeForm()
        {
            this.Text = "CoreBrush - Editor de Imagens - Renan Lapazini";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arquivos de Imagem|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Todos os arquivos|*.*";
            
            saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG|*.png|JPEG|*.jpg|BMP|*.bmp|Todos os arquivos|*.*";
        }

        private void CreateMenus()
        {
            menuStrip = new MenuStrip();
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            // Menu ARQUIVO
            var fileMenu = new ToolStripMenuItem("&Arquivo");
            fileMenu.DropDownItems.Add("&Abrir imagem", null, OpenImage_Click);
            fileMenu.DropDownItems.Add("&Salvar imagem", null, SaveImage_Click);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("&Sobre", null, About_Click);
            fileMenu.DropDownItems.Add("&Sair", null, Exit_Click);

            // Menu TRANSFORMAÇÕES GEOMÉTRICAS
            var geometricMenu = new ToolStripMenuItem("&Transformações Geométricas");
            geometricMenu.DropDownItems.Add("&Transladar", null, Translate_Click);
            geometricMenu.DropDownItems.Add("&Rotacionar", null, Rotate_Click);
            geometricMenu.DropDownItems.Add("&Espelhar", null, Mirror_Click);
            geometricMenu.DropDownItems.Add("A&umentar", null, Increase_Click);
            geometricMenu.DropDownItems.Add("&Diminuir", null, Decrease_Click);

            // Menu FILTROS
            var filtersMenu = new ToolStripMenuItem("&Filtros");
            filtersMenu.DropDownItems.Add("&Brilho e Contraste", null, BrightnessContrast_Click);
            filtersMenu.DropDownItems.Add("&Grayscale", null, Grayscale_Click);
            filtersMenu.DropDownItems.Add("&Passa Baixa", null, LowPass_Click);
            filtersMenu.DropDownItems.Add("Passa &Alta", null, HighPass_Click);
            filtersMenu.DropDownItems.Add("&Threshold", null, Threshold_Click);
            filtersMenu.DropDownItems.Add("A&finamento", null, Thinning_Click);

            // Menu MORFOLOGIA MATEMÁTICA
            var morphologyMenu = new ToolStripMenuItem("&Morfologia Matemática");
            morphologyMenu.DropDownItems.Add("&Dilatação", null, Dilation_Click);
            morphologyMenu.DropDownItems.Add("&Erosão", null, Erosion_Click);
            morphologyMenu.DropDownItems.Add("&Abertura", null, Opening_Click);
            morphologyMenu.DropDownItems.Add("&Fechamento", null, Closing_Click);

            // Menu EXTRAÇÃO DE CARACTERÍSTICAS
            var featuresMenu = new ToolStripMenuItem("&Extração de Características");
            featuresMenu.DropDownItems.Add("&Desafio", null, Challenge_Click);

            menuStrip.Items.AddRange(new ToolStripItem[]
            {
                fileMenu,
                geometricMenu,
                filtersMenu,
                morphologyMenu,
                featuresMenu
            });
        }

        private void CreatePictureBoxes()
        {
            // Label para imagem original
            originalLabel = new Label();
            originalLabel.Text = "Imagem Original";
            originalLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            originalLabel.Location = new Point(20, menuStrip.Height + 10);
            originalLabel.Size = new Size(200, 25);
            this.Controls.Add(originalLabel);

            // PictureBox para imagem original
            originalImageBox = new PictureBox();
            originalImageBox.Location = new Point(20, originalLabel.Bottom + 10);
            originalImageBox.Size = new Size(580, 400);
            originalImageBox.BorderStyle = BorderStyle.FixedSingle;
            originalImageBox.SizeMode = PictureBoxSizeMode.Zoom;
            originalImageBox.BackColor = Color.White;
            this.Controls.Add(originalImageBox);

            // Label para imagem transformada
            transformedLabel = new Label();
            transformedLabel.Text = "Imagem Transformada";
            transformedLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            transformedLabel.Location = new Point(620, menuStrip.Height + 10);
            transformedLabel.Size = new Size(200, 25);
            this.Controls.Add(transformedLabel);

            // PictureBox para imagem transformada
            transformedImageBox = new PictureBox();
            transformedImageBox.Location = new Point(620, transformedLabel.Bottom + 10);
            transformedImageBox.Size = new Size(580, 400);
            transformedImageBox.BorderStyle = BorderStyle.FixedSingle;
            transformedImageBox.SizeMode = PictureBoxSizeMode.CenterImage;
            transformedImageBox.BackColor = Color.White;
            this.Controls.Add(transformedImageBox);

            // Ajustar o layout quando a janela for redimensionada
            this.Resize += MainForm_Resize;
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            int width = (this.ClientSize.Width - 60) / 2;
            int height = this.ClientSize.Height - menuStrip.Height - 80;

            originalImageBox.Size = new Size(width, height);
            transformedImageBox.Location = new Point(width + 40, transformedLabel.Bottom + 10);
            transformedImageBox.Size = new Size(width, height);
            transformedLabel.Location = new Point(width + 40, menuStrip.Height + 10);
        }

        #region Menu Event Handlers

        private void OpenImage_Click(object? sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    originalImage = new Bitmap(openFileDialog.FileName);
                    originalImageBox.Image = originalImage;
                    transformedImage = new Bitmap(originalImage);
                    transformedImageBox.SizeMode = PictureBoxSizeMode.CenterImage;
                    transformedImageBox.Size = originalImageBox.Size;
                    transformedImageBox.Image = transformedImage;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao abrir a imagem: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SaveImage_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem para salvar!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    transformedImage.Save(saveFileDialog.FileName);
                    MessageBox.Show("Imagem salva com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao salvar a imagem: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void About_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("CoreBrush - Editor de Imagens\n\nDesenvolvido por: Renan Lapazini\nVersão: 1.0\n\nSoftware para edição e processamento de imagens com filtros e transformações geométricas.", 
                "Sobre", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        // Transformações Geométricas
        private void Translate_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Solicita deslocamento X e Y
            var dxStr = Microsoft.VisualBasic.Interaction.InputBox("Deslocamento em X:", "Transladar", "50");
            if (string.IsNullOrWhiteSpace(dxStr)) return;
            var dyStr = Microsoft.VisualBasic.Interaction.InputBox("Deslocamento em Y:", "Transladar", "50");
            if (string.IsNullOrWhiteSpace(dyStr)) return;
            if (!int.TryParse(dxStr, out int dx) || !int.TryParse(dyStr, out int dy))
            {
                MessageBox.Show("Valores inválidos.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Bitmap bmp = new Bitmap(transformedImage.Width, transformedImage.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.DrawImage(transformedImage, dx, dy);
            }
            transformedImage?.Dispose();
            transformedImage = bmp;
            transformedImageBox.Image = transformedImage;
        }

        private void Rotate_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var angleStr = Microsoft.VisualBasic.Interaction.InputBox("Ângulo de rotação (graus):", "Rotacionar", "90");
            if (string.IsNullOrWhiteSpace(angleStr)) return;
            if (!float.TryParse(angleStr, out float angle))
            {
                MessageBox.Show("Valor inválido.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            double rad = angle * Math.PI / 180.0;
            int w = transformedImage.Width;
            int h = transformedImage.Height;
            double cos = Math.Abs(Math.Cos(rad));
            double sin = Math.Abs(Math.Sin(rad));
            int newW = (int)Math.Round(w * cos + h * sin);
            int newH = (int)Math.Round(h * cos + w * sin);
            Bitmap bmp = new Bitmap(newW, newH);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                // Centraliza a imagem rotacionada
                g.TranslateTransform(newW / 2f, newH / 2f);
                g.RotateTransform(angle);
                g.TranslateTransform(-w / 2f, -h / 2f);
                g.DrawImage(transformedImage, 0, 0);
            }
            transformedImage?.Dispose();
            transformedImage = bmp;
            transformedImageBox.Image = transformedImage;
        }

        private void Mirror_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var axis = MessageBox.Show("Espelhar horizontalmente? (Sim = Horizontal, Não = Vertical)", "Espelhar", MessageBoxButtons.YesNoCancel);
            if (axis == DialogResult.Cancel) return;
            Bitmap bmp = new Bitmap(transformedImage.Width, transformedImage.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                if (axis == DialogResult.Yes)
                {
                    g.ScaleTransform(-1, 1);
                    g.TranslateTransform(-transformedImage.Width, 0);
                }
                else
                {
                    g.ScaleTransform(1, -1);
                    g.TranslateTransform(0, -transformedImage.Height);
                }
                g.DrawImage(transformedImage, 0, 0);
            }
            transformedImage?.Dispose();
            transformedImage = bmp;
            transformedImageBox.Image = transformedImage;
        }

        private void Increase_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var factorStr = Microsoft.VisualBasic.Interaction.InputBox("Fator de aumento:", "Aumentar", "1.5");
            if (string.IsNullOrWhiteSpace(factorStr)) return;
            if (!float.TryParse(factorStr, out float factor) || factor <= 0)
            {
                MessageBox.Show("Valor inválido.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int newW = (int)(transformedImage.Width * factor);
            int newH = (int)(transformedImage.Height * factor);
            Bitmap bmp = new Bitmap(newW, newH);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(transformedImage, 0, 0, newW, newH);
            }
            transformedImage?.Dispose();
            transformedImage = bmp;
            transformedImageBox.Image = transformedImage;
        }

        private void Decrease_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var factorStr = Microsoft.VisualBasic.Interaction.InputBox("Fator de diminuição:", "Diminuir", "0.5");
            if (string.IsNullOrWhiteSpace(factorStr)) return;
            factorStr = factorStr.Replace(',', '.');
            float factor;
            bool ok = float.TryParse(factorStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out factor);
            if (!ok || factor <= 0 || factor >= 1)
            {
                MessageBox.Show("Valor inválido. Use um valor entre 0 e 1.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int newW = Math.Max(1, (int)Math.Round(transformedImage.Width * factor));
            int newH = Math.Max(1, (int)Math.Round(transformedImage.Height * factor));
            Bitmap bmp = new Bitmap(newW, newH);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(transformedImage, 0, 0, newW, newH);
            }
            transformedImage?.Dispose();
            transformedImage = bmp;
            transformedImageBox.SizeMode = PictureBoxSizeMode.CenterImage;
            transformedImageBox.Image = null;
            transformedImageBox.Image = transformedImage;
            transformedImageBox.Refresh();
        }

        // Filtros
        private void BrightnessContrast_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidade de Brilho e Contraste será implementada.", "Em desenvolvimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Grayscale_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidade de Grayscale será implementada.", "Em desenvolvimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LowPass_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidade de Filtro Passa Baixa será implementada.", "Em desenvolvimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void HighPass_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidade de Filtro Passa Alta será implementada.", "Em desenvolvimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Threshold_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidade de Threshold será implementada.", "Em desenvolvimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Thinning_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidade de Afinamento será implementada.", "Em desenvolvimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Morfologia Matemática
        private void Dilation_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidade de Dilatação será implementada.", "Em desenvolvimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Erosion_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidade de Erosão será implementada.", "Em desenvolvimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Opening_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidade de Abertura será implementada.", "Em desenvolvimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Closing_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidade de Fechamento será implementada.", "Em desenvolvimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Extração de Características
        private void Challenge_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidade de Desafio será implementada.", "Em desenvolvimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                originalImage?.Dispose();
                transformedImage?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
