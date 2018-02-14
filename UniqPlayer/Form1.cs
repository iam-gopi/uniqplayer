﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace UniqPlayer
{
    public partial class UniqPlayer : Form
    {
        private const string SKey = "_?73^?dVT3st5har3";
        private const string SaltKey = "!2S@LT&KT3st5har3EY";
        private const int Iterations = 1042;
        string destination = string.Empty;

        public UniqPlayer()
        {
            InitializeComponent();
        }

        private void UniqPlayer_Load(object sender, EventArgs e)
        {
            mediaPlayer.Dock = DockStyle.Fill;
        }

        private void encryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] fileNames = null;
            EncryptFile(fileNames);
        }

        public static void EncryptFile(string[] fileNames)
        {
            //string srcFilename = @"E:\Study Material\Video Tutorials\lamda.mp4";
            string destFilename = string.Empty; // @"C:\Users\Gopi\Desktop\New folder\test.guv";

            if (fileNames == null)
            {
                UniqPlayer p = new UniqPlayer();
                fileNames = p.OpenDialogFunctionalities();
            }

            UniqPlayer uniqPlayerObject = new UniqPlayer();
            string drive = uniqPlayerObject.GetAvailableDrivesOnSystem();
            int existingFileCounts = 0;

            if (drive == @"C:\")
                destFilename = @"C:\Users\" + Environment.UserName + @"\Desktop\Encrypted Files\";
            else
                destFilename = drive + @"Encrypted Files\";

            if (!Directory.Exists(destFilename))
            {
                Directory.CreateDirectory(destFilename);
                Directory.SetAccessControl(destFilename, new DirectorySecurity());

                DirectoryInfo info = new DirectoryInfo(destFilename);
                DirectorySecurity security = info.GetAccessControl();
                security.AddAccessRule(new FileSystemAccessRule(Environment.UserName, FileSystemRights.FullControl, AccessControlType.Allow));

                info.SetAccessControl(security);
            }
            else
            {
                existingFileCounts = (Directory.GetFiles(destFilename, "*", SearchOption.TopDirectoryOnly).Length) > 0 ? Directory.GetFiles(destFilename, "*", SearchOption.TopDirectoryOnly).Length : 0;
            }

            string _fileName = destFilename;
            int limitValue = existingFileCounts + fileNames.Length;
            string temp = string.Empty;

            for (int i = existingFileCounts; i < limitValue; i++)
            {
                destFilename += "enc_" + i + ".guv";
                var aes = new AesManaged();
                aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
                aes.KeySize = aes.LegalKeySizes[0].MaxSize;
                var salt = GetBytes(SaltKey);
                var key = new Rfc2898DeriveBytes(SKey, salt, Iterations);
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                aes.Mode = CipherMode.CBC;

                try
                {
                    //for (int j = 0; j < fileNames.Length; j++)
                    //{
                    //    temp = fileNames[j];
                    //}

                    ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);
                    using (var dest = new System.IO.FileStream(destFilename, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        using (var cryptoStream = new CryptoStream(dest, transform, CryptoStreamMode.Write))
                        {
                            //TODO: filename needs to be corrected.
                            using (var source = new FileStream(fileNames[i], FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                source.CopyTo(cryptoStream);
                            }
                        }
                    }

                    destFilename = _fileName;
                }
                catch (Exception ex)
                {

                }
            }
            MessageBox.Show("All the selected file are encrypted successfully on " + destFilename + " path.", "UniqPlayer - Guvi", MessageBoxButtons.OK);
            Process.Start(destFilename);
        }

        static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private void decryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DecryptFile();
        }

        private void DecryptFile()
        {
            string srcFilename = string.Empty;
            string destFilename = string.Empty;
            string[] filesInPath = null;
            int totalFiles = 0;

            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            DialogResult result = folderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string selectedPath = folderDialog.SelectedPath;

                filesInPath = Directory.GetFiles(selectedPath);
                totalFiles = Directory.GetFiles(selectedPath).Length;

                if (!Directory.Exists(selectedPath + @"\Decrypted Files"))
                { Directory.CreateDirectory(selectedPath + @"\Decrypted Files"); }

                destFilename = selectedPath + @"\Decrypted Files\";
                srcFilename = selectedPath + @"\";
            }
            else { return; }
            string tempFilePath = destFilename;

            WMPLib.IWMPPlaylist playlist = mediaPlayer.playlistCollection.newPlaylist("myplaylist");
            WMPLib.IWMPMedia media;
            //if (ofdSong.ShowDialog() == DialogResult.OK)
            //{
            //    foreach (string file in ofdSong.FileNames)
            //    {
            //        media = mediaPlayer.newMedia(file);
            //        playlist.appendItem(media);
            //    }
            //}
            //wmp.currentPlaylist = playlist;
            //wmp.Ctlcontrols.play();



            for (int i = 0; i < totalFiles; i++)
            {
                destFilename += "dec_" + i + ".mp4";

                var aes = new AesManaged();
                aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
                aes.KeySize = aes.LegalKeySizes[0].MaxSize;
                var salt = GetBytes(SaltKey);
                var key = new Rfc2898DeriveBytes(SKey, salt, Iterations);
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                aes.Mode = CipherMode.CBC;
                ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);

                try
                {
                    using (var dest = new FileStream(destFilename, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        using (var cryptoStream = new CryptoStream(dest, transform, CryptoStreamMode.Write))
                        {
                            try
                            {
                                using (var source = new FileStream(filesInPath[i], FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    source.CopyTo(cryptoStream);

                                    media = mediaPlayer.newMedia(destFilename);
                                    playlist.appendItem(media);
                                }
                            }
                            catch (CryptographicException exception)
                            {
                                throw new ApplicationException("Decryption failed.", exception);
                            }
                        }
                    }
                    destFilename = tempFilePath;
                }
                catch (Exception ex) { }
            }
            mediaPlayer.currentPlaylist = playlist;
            mediaPlayer.Ctlcontrols.play();
        }

        //close the application entirely
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenDialogFunctionalities();
        }

        private string GetAvailableDrivesOnSystem()
        {
            DriveInfo[] availableDrives = DriveInfo.GetDrives();
            return availableDrives.Select(x => x).FirstOrDefault().ToString();
        }

        private string[] OpenDialogFunctionalities()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Multiselect = true;
            openDialog.InitialDirectory = GetAvailableDrivesOnSystem();
            openDialog.Filter = "MP4  (*.mp4)|*.mp4";
            openDialog.RestoreDirectory = true;
            openDialog.Title = "Select MP4 files - Guvi";

            string[] selectedFiles = openDialog.FileNames;

            if (openDialog.ShowDialog() == DialogResult.OK)
                EncryptFile(openDialog.FileNames);

            return selectedFiles;
        }
    }
}
