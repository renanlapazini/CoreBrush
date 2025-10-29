using System;
using System.Collections.Generic;
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
    private HistoryManager history = new HistoryManager { Capacity = 10 }; // Histórico de transformações

        public MainForm()
        {
            InitializeComponent();
            InitializeForm();
            CreateMenus();
            CreatePictureBoxes();
            // Configurar atalho Ctrl+Z
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                Undo_Click(sender, e);
                e.Handled = true;
            }
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
            morphologyMenu.DropDownItems.Add("&Esqueletização", null, Skeletonization_Click);

            // Menu EXTRAÇÃO DE CARACTERÍSTICAS
            var featuresMenu = new ToolStripMenuItem("&Extração de Características");
            featuresMenu.DropDownItems.Add("&Desafio", null, Challenge_Click);

            // Menu EDITAR
            var editMenu = new ToolStripMenuItem("&Editar");
            editMenu.DropDownItems.Add("&Desfazer\tCtrl+Z", null, Undo_Click);

            menuStrip.Items.AddRange(new ToolStripItem[]
            {
                fileMenu,
                geometricMenu,
                filtersMenu,
                morphologyMenu,
                featuresMenu,
                editMenu
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
            transformedImageBox.SizeMode = PictureBoxSizeMode.Zoom;
            transformedImageBox.BackColor = Color.White;
            this.Controls.Add(transformedImageBox);

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
                    transformedImageBox.Size = originalImageBox.Size; // manter proporção do layout
                    AdjustDisplayModeForImage(transformedImage);
                    transformedImageBox.Image = transformedImage;
                    // Limpar histórico e adicionar estado inicial
                    history.Clear();
                    history.Push(transformedImage);
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
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
            // Salvar novo estado no histórico
            SaveToHistory();
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
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
            SaveToHistory();
        }

        private void Mirror_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            Bitmap bmp = new Bitmap(transformedImage.Width, transformedImage.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                // Espelhamento horizontal
                g.ScaleTransform(-1, 1);
                g.TranslateTransform(-transformedImage.Width, 0);
                g.DrawImage(transformedImage, 0, 0);
            }
            transformedImage?.Dispose();
            transformedImage = bmp;
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
            SaveToHistory();
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
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
            SaveToHistory();
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
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = null;
            transformedImageBox.Image = transformedImage;
            transformedImageBox.Refresh();
            SaveToHistory();
        }

        // Ajusta dinamicamente o modo de exibição conforme o tamanho relativo da imagem ao PictureBox
        private void AdjustDisplayModeForImage(Bitmap img)
        {
            if (img.Width > transformedImageBox.Width || img.Height > transformedImageBox.Height)
                transformedImageBox.SizeMode = PictureBoxSizeMode.Zoom; // imagem maior: ajustar para caber
            else
                transformedImageBox.SizeMode = PictureBoxSizeMode.CenterImage; // imagem menor: mostrar no tamanho real centralizada
        }

        // Filtros
        private void BrightnessContrast_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Solicita valores de brilho e contraste
            var brightnessStr = Microsoft.VisualBasic.Interaction.InputBox("Ajuste de brilho (-100 a 100):", "Brilho e Contraste", "0");
            if (string.IsNullOrWhiteSpace(brightnessStr)) return;
            var contrastStr = Microsoft.VisualBasic.Interaction.InputBox("Ajuste de contraste (0.1 a 3.0):", "Brilho e Contraste", "1.0");
            if (string.IsNullOrWhiteSpace(contrastStr)) return;

            if (!int.TryParse(brightnessStr, out int brightness) || brightness < -100 || brightness > 100)
            {
                MessageBox.Show("Valor de brilho inválido. Use valores entre -100 e 100.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            contrastStr = contrastStr.Replace(',', '.');
            if (!float.TryParse(contrastStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float contrast) || contrast < 0.1f || contrast > 3.0f)
            {
                MessageBox.Show("Valor de contraste inválido. Use valores between 0.1 e 3.0.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var bc = ImageProcessor.BrightnessContrast(transformedImage, brightness, contrast);
            transformedImage?.Dispose();
            transformedImage = bc;
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
            SaveToHistory();
        }

        private void Grayscale_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var grayBmp = ImageProcessor.Grayscale(transformedImage);
            transformedImage?.Dispose();
            transformedImage = grayBmp;
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
            SaveToHistory();
        }

        private void LowPass_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var choice = Interaction.InputBox(
                "Escolha o filtro Passa-Baixa:\n1 - Média (3x3)\n2 - Gaussiano (5x5)",
                "Filtro Passa-Baixa",
                "1");
            if (string.IsNullOrWhiteSpace(choice)) return;

            Bitmap result;
            if (choice.Trim() == "2")
            {
                // Gaussiano 5x5 (sigma ~= 1), fator = 1/256
                double[,] kernel = new double[,]
                {
                    { 1,  4,  6,  4, 1 },
                    { 4, 16, 24, 16, 4 },
                    { 6, 24, 36, 24, 6 },
                    { 4, 16, 24, 16, 4 },
                    { 1,  4,  6,  4, 1 }
                };
                result = ImageProcessor.ApplyConvolution(transformedImage, kernel, factor: 1.0 / 256.0, bias: 0.0);
            }
            else
            {
                // Média 3x3 (box blur), fator = 1/9
                double[,] kernel = new double[,]
                {
                    { 1, 1, 1 },
                    { 1, 1, 1 },
                    { 1, 1, 1 }
                };
                result = ImageProcessor.ApplyConvolution(transformedImage, kernel, factor: 1.0 / 9.0, bias: 0.0);
            }

            transformedImage?.Dispose();
            transformedImage = result;
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
            SaveToHistory();
        }

        private void HighPass_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var choice = Interaction.InputBox(
                "Escolha o filtro Passa-Alta:\n1 - Laplaciano (bordas)\n2 - Unsharp Mask (realce)",
                "Filtro Passa-Alta",
                "2");
            if (string.IsNullOrWhiteSpace(choice)) return;

            Bitmap result;
            if (choice.Trim() == "1")
            {
                // Laplaciano 3x3 (8-vizinhos), fator = 1
                double[,] kernel = new double[,]
                {
                    { -1, -1, -1 },
                    { -1,  8, -1 },
                    { -1, -1, -1 }
                };
                // Aplicar laplaciano para extrair bordas e normalizar para visualização
                var edges = ImageProcessor.ApplyConvolution(transformedImage, kernel, factor: 1.0, bias: 0.0, clampToGrayScale: true, absoluteValue: true);
                result = edges;
            }
            else
            {
                // Unsharp mask: result = original + amount * (original - blur(original))
                var amountStr = Interaction.InputBox("Fator de realce (0.5 a 2.0):", "Unsharp Mask", "1.0");
                if (string.IsNullOrWhiteSpace(amountStr)) return;
                amountStr = amountStr.Replace(',', '.');
                if (!double.TryParse(amountStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double amount) || amount < 0.5 || amount > 2.0)
                {
                    MessageBox.Show("Valor inválido. Use um valor entre 0.5 e 2.0.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Gaussian blur 5x5 para criar a máscara
                double[,] g = new double[,]
                {
                    { 1,  4,  6,  4, 1 },
                    { 4, 16, 24, 16, 4 },
                    { 6, 24, 36, 24, 6 },
                    { 4, 16, 24, 16, 4 },
                    { 1,  4,  6,  4, 1 }
                };
                using var blurred = ImageProcessor.ApplyConvolution(transformedImage, g, factor: 1.0 / 256.0, bias: 0.0);
                result = ImageProcessor.ApplyUnsharp(transformedImage, blurred, amount);
            }

            transformedImage?.Dispose();
            transformedImage = result;
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
            SaveToHistory();
        }

        private void Threshold_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var choice = Interaction.InputBox(
                "Escolha o método de Threshold:\n1 - Manual (0-255)\n2 - Otsu (automático)\n3 - Adaptativo (Gaussiano 5x5)",
                "Thresholding",
                "2");
            if (string.IsNullOrWhiteSpace(choice)) return;

            Bitmap result;
            switch (choice.Trim())
            {
                case "1":
                    var tStr = Interaction.InputBox("Valor do limiar (0-255):", "Threshold Manual", "128");
                    if (string.IsNullOrWhiteSpace(tStr)) return;
                    if (!int.TryParse(tStr, out int t) || t < 0 || t > 255)
                    {
                        MessageBox.Show("Valor inválido. Use 0 a 255.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    result = ImageProcessor.ThresholdManual(transformedImage, t);
                    break;
                case "3":
                    var cStr = Interaction.InputBox("Constante C (desloca o limiar local):", "Threshold Adaptativo", "5");
                    if (string.IsNullOrWhiteSpace(cStr)) return;
                    cStr = cStr.Replace(',', '.');
                    if (!double.TryParse(cStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double C))
                    {
                        MessageBox.Show("Valor inválido para C.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    result = ImageProcessor.ThresholdAdaptiveGaussian(transformedImage, C);
                    break;
                case "2":
                default:
                    int otsuT = ImageProcessor.ComputeOtsuThreshold(transformedImage);
                    result = ImageProcessor.ThresholdManual(transformedImage, otsuT);
                    break;
            }

            transformedImage?.Dispose();
            transformedImage = result;
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
            SaveToHistory();
        }

        private void Thinning_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidade de Afinamento será implementada.", "Em desenvolvimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Morfologia Matemática
        private void Dilation_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var choice = Interaction.InputBox(
                "Dilatação:\n1 - Binária (preencher buracos, unir)\n2 - Tons de Cinza (max 3x3)",
                "Dilatação",
                "1");
            if (string.IsNullOrWhiteSpace(choice)) return;

            Bitmap result = choice.Trim() == "2"
                ? ImageProcessor.DilateGrayscale(transformedImage)
                : ImageProcessor.DilateBinary(transformedImage);

            transformedImage?.Dispose();
            transformedImage = result;
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
            SaveToHistory();
        }

        private void Erosion_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var choice = Interaction.InputBox(
                "Erosão:\n1 - Binária (encolher/limpar ruído)\n2 - Tons de Cinza (min 3x3)",
                "Erosão",
                "1");
            if (string.IsNullOrWhiteSpace(choice)) return;

            Bitmap result = choice.Trim() == "2"
                ? ImageProcessor.ErodeGrayscale(transformedImage)
                : ImageProcessor.ErodeBinary(transformedImage);

            transformedImage?.Dispose();
            transformedImage = result;
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
            SaveToHistory();
        }

        private void Opening_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var choice = Interaction.InputBox(
                "Abertura (Erosão -> Dilatação) com SE 3x3:\n1 - Binária\n2 - Tons de Cinza",
                "Abertura",
                "1");
            if (string.IsNullOrWhiteSpace(choice)) return;

            Bitmap result;
            if (choice.Trim() == "2")
            {
                using var eroded = ImageProcessor.ErodeGrayscale(transformedImage);
                result = ImageProcessor.DilateGrayscale(eroded);
            }
            else
            {
                using var eroded = ImageProcessor.ErodeBinary(transformedImage);
                result = ImageProcessor.DilateBinary(eroded);
            }

            transformedImage?.Dispose();
            transformedImage = result;
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
            SaveToHistory();
        }

        private void Closing_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var choice = Interaction.InputBox(
                "Fechamento (Dilatação -> Erosão) com SE 3x3:\n1 - Binária\n2 - Tons de Cinza",
                "Fechamento",
                "1");
            if (string.IsNullOrWhiteSpace(choice)) return;

            Bitmap result;
            if (choice.Trim() == "2")
            {
                using var dilated = ImageProcessor.DilateGrayscale(transformedImage);
                result = ImageProcessor.ErodeGrayscale(dilated);
            }
            else
            {
                using var dilated = ImageProcessor.DilateBinary(transformedImage);
                result = ImageProcessor.ErodeBinary(dilated);
            }

            transformedImage?.Dispose();
            transformedImage = result;
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
            SaveToHistory();
        }

        // Extração de Características
        private void Challenge_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidade de Desafio será implementada.", "Em desenvolvimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Skeletonization_Click(object? sender, EventArgs e)
        {
            if (transformedImage == null)
            {
                MessageBox.Show("Nenhuma imagem carregada!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Verifica rapidamente se a imagem parece binária (0/255) amostrando alguns pixels
            bool isBinary = true;
            for (int y = 0; y < transformedImage.Height && y < 10 && isBinary; y++)
            {
                for (int x = 0; x < transformedImage.Width && x < 10; x++)
                {
                    var p = transformedImage.GetPixel(x, y);
                    int gray = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
                    if (!(gray == 0 || gray == 255)) { isBinary = false; break; }
                }
            }

            if (!isBinary)
            {
                var resp = Interaction.InputBox("A imagem não parece binária. Aplicar Otsu antes? (1=Sim, 2=Não)", "Esqueletização", "1");
                if (string.IsNullOrWhiteSpace(resp)) return;
                if (resp.Trim() == "1")
                {
                    int t = ImageProcessor.ComputeOtsuThreshold(transformedImage);
                    using var bin = ImageProcessor.ThresholdManual(transformedImage, t);
                    transformedImage?.Dispose();
                    transformedImage = new Bitmap(bin);
                }
            }

            // Garantir que o objeto esteja em branco (foreground=255) e fundo preto, invertendo se necessário
            int white = 0, black = 0;
            for (int y = 0; y < transformedImage.Height; y += Math.Max(1, transformedImage.Height / 200))
            {
                for (int x = 0; x < transformedImage.Width; x += Math.Max(1, transformedImage.Width / 200))
                {
                    var p = transformedImage.GetPixel(x, y);
                    int gray = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
                    if (gray >= 128) white++; else black++;
                }
            }
            // Se a maioria é branca, provavelmente o fundo é branco e o objeto é preto -> inverter
            if (white > black)
            {
                using var inv = ImageProcessor.Invert(transformedImage);
                transformedImage?.Dispose();
                transformedImage = new Bitmap(inv);
            }

            using var skel = ImageProcessor.SkeletonizeZhangSuen(transformedImage);
            transformedImage?.Dispose();
            transformedImage = new Bitmap(skel);
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
            SaveToHistory();
        }

        // Métodos de histórico e desfazer
        private void SaveToHistory()
        {
            if (transformedImage == null) return;
            history.Push(transformedImage);
        }

        private void ClearHistory()
        {
            history.Clear();
        }

        private void Undo_Click(object? sender, EventArgs e)
        {
            if (!history.CanUndo)
            {
                MessageBox.Show("Nenhuma operação para desfazer!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Restaura a imagem anterior via HistoryManager
            transformedImage?.Dispose();
            transformedImage = history.Undo();
            AdjustDisplayModeForImage(transformedImage);
            transformedImageBox.Image = transformedImage;
        }

        #endregion

        // ============================
        // Utilidades de Processamento
        // ============================

        // Convolução genérica RGB, com opções para valor absoluto (bordas) e conversão para escala de cinza
        private static Bitmap ApplyConvolution(Bitmap source, double[,] kernel, double factor = 1.0, double bias = 0.0, bool clampToGrayScale = false, bool absoluteValue = false)
        {
            int width = source.Width;
            int height = source.Height;
            Bitmap result = new Bitmap(width, height);

            int kH = kernel.GetLength(0); // linhas
            int kW = kernel.GetLength(1); // colunas
            int kHalfW = kW / 2;
            int kHalfH = kH / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double r = 0, g = 0, b = 0;
                    int a = source.GetPixel(x, y).A;

                    for (int ky = 0; ky < kH; ky++)
                    {
                        int sy = Math.Clamp(y + ky - kHalfH, 0, height - 1);
                        for (int kx = 0; kx < kW; kx++)
                        {
                            int sx = Math.Clamp(x + kx - kHalfW, 0, width - 1);
                            Color sc = source.GetPixel(sx, sy);
                            double kval = kernel[ky, kx];
                            r += sc.R * kval;
                            g += sc.G * kval;
                            b += sc.B * kval;
                        }
                    }

                    if (absoluteValue)
                    {
                        r = Math.Abs(r);
                        g = Math.Abs(g);
                        b = Math.Abs(b);
                    }

                    r = r * factor + bias;
                    g = g * factor + bias;
                    b = b * factor + bias;

                    int ri = (int)Math.Round(Math.Clamp(r, 0, 255));
                    int gi = (int)Math.Round(Math.Clamp(g, 0, 255));
                    int bi = (int)Math.Round(Math.Clamp(b, 0, 255));

                    if (clampToGrayScale)
                    {
                        int gray = (int)Math.Round(0.299 * ri + 0.587 * gi + 0.114 * bi);
                        gray = Math.Max(0, Math.Min(255, gray));
                        result.SetPixel(x, y, Color.FromArgb(a, gray, gray, gray));
                    }
                    else
                    {
                        result.SetPixel(x, y, Color.FromArgb(a, ri, gi, bi));
                    }
                }
            }

            return result;
        }

        // Realce tipo Unsharp Mask: out = original + amount * (original - blurred)
        private static Bitmap ApplyUnsharp(Bitmap original, Bitmap blurred, double amount)
        {
            int width = original.Width;
            int height = original.Height;
            Bitmap? resized = null;
            Bitmap blurRef = blurred;
            if (blurred.Width != width || blurred.Height != height)
            {
                // Ajusta com redimensionamento básico
                resized = new Bitmap(original.Width, original.Height);
                using (Graphics g = Graphics.FromImage(resized))
                {
                    g.DrawImage(blurred, 0, 0, original.Width, original.Height);
                }
                blurRef = resized;
            }

            Bitmap result = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color o = original.GetPixel(x, y);
                    Color b = blurRef.GetPixel(x, y);
                    double r = o.R + amount * (o.R - b.R);
                    double g = o.G + amount * (o.G - b.G);
                    double bl = o.B + amount * (o.B - b.B);
                    int ri = (int)Math.Round(Math.Clamp(r, 0, 255));
                    int gi = (int)Math.Round(Math.Clamp(g, 0, 255));
                    int bi = (int)Math.Round(Math.Clamp(bl, 0, 255));
                    result.SetPixel(x, y, Color.FromArgb(o.A, ri, gi, bi));
                }
            }
            resized?.Dispose();
            return result;
        }

        // ============================
        // Thresholding helpers
        // ============================

        // Aplica threshold binário usando luminância (independe de ser colorido ou cinza)
        private static Bitmap ApplyThresholdManual(Bitmap source, int threshold)
        {
            int w = source.Width;
            int h = source.Height;
            Bitmap result = new Bitmap(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var p = source.GetPixel(x, y);
                    int gray = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
                    int bw = gray >= threshold ? 255 : 0;
                    result.SetPixel(x, y, Color.FromArgb(p.A, bw, bw, bw));
                }
            }
            return result;
        }

        // Calcula limiar de Otsu sobre a luminância
        private static int ComputeOtsuThreshold(Bitmap source)
        {
            // Histograma de 256 níveis
            int[] hist = new int[256];
            int w = source.Width;
            int h = source.Height;
            int total = w * h;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var p = source.GetPixel(x, y);
                    int gray = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
                    hist[gray]++;
                }
            }

            double sum = 0;
            for (int i = 0; i < 256; i++) sum += i * hist[i];

            double sumB = 0;
            int wB = 0;
            int wF = 0;
            double maxVar = -1;
            int threshold = 0;

            for (int t = 0; t < 256; t++)
            {
                wB += hist[t];
                if (wB == 0) continue;
                wF = total - wB;
                if (wF == 0) break;

                sumB += t * hist[t];
                double mB = sumB / wB;
                double mF = (sum - sumB) / wF;
                double between = wB * wF * (mB - mF) * (mB - mF);
                if (between > maxVar)
                {
                    maxVar = between;
                    threshold = t;
                }
            }
            return threshold;
        }

        // Threshold adaptativo usando média gaussiana local 5x5 menos uma constante C
        private static Bitmap ApplyThresholdAdaptiveGaussian(Bitmap source, double C)
        {
            // Suaviza (média gaussiana) e cria um mapa de limiares locais
            double[,] g5 = new double[,]
            {
                { 1,  4,  6,  4, 1 },
                { 4, 16, 24, 16, 4 },
                { 6, 24, 36, 24, 6 },
                { 4, 16, 24, 16, 4 },
                { 1,  4,  6,  4, 1 }
            };
            using var localMean = ApplyConvolution(source, g5, factor: 1.0 / 256.0, bias: 0.0, clampToGrayScale: true);

            int w = source.Width;
            int h = source.Height;
            Bitmap result = new Bitmap(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var p = source.GetPixel(x, y);
                    int gray = (int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B);
                    int tLocal = (int)Math.Round(localMean.GetPixel(x, y).R - C);
                    tLocal = Math.Max(0, Math.Min(255, tLocal));
                    int bw = gray >= tLocal ? 255 : 0;
                    result.SetPixel(x, y, Color.FromArgb(p.A, bw, bw, bw));
                }
            }
            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                originalImage?.Dispose();
                transformedImage?.Dispose();
                history.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
