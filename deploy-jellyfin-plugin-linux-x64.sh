#!/usr/bin/env bash
set -euo pipefail

PLUGIN_NAME="Jellyfin2HQPlayer"
TARBALL=$(ls -t Jellyfin2HQPlayerPlugin-*.tar.gz 2>/dev/null | head -n 1)

INSTALL_DIR="/var/lib/jellyfin/plugins/${PLUGIN_NAME}"
TMP_DIR="/tmp/${PLUGIN_NAME}_deploy"

echo "=========================================="
echo " Jellyfin Plugin Deploy Script"
echo "=========================================="

# 1. 检查包
if [ -z "$TARBALL" ]; then
  echo "❌ No Jellyfin2HQPlayerPlugin-*.tar.gz found in current directory"
  exit 1
fi

echo "✔ Found package: $TARBALL"

# 2. 清理临时目录
rm -rf "$TMP_DIR"
mkdir -p "$TMP_DIR"

# 3. 解压
echo "➡ Extracting..."
tar -xzf "$TARBALL" -C "$TMP_DIR"

# 4. 找到实际目录（兼容结构）
PLUGIN_SRC=$(find "$TMP_DIR" -type d -name "Jellyfin2HQPlayerPlugin" | head -n 1)

if [ -z "$PLUGIN_SRC" ]; then
  echo "❌ Cannot find Jellyfin2HQPlayerPlugin directory inside tar.gz"
  exit 1
fi

# 5. 创建插件目录
echo "➡ Creating plugin directory..."
sudo mkdir -p "$INSTALL_DIR"

# 6. 清空旧版本（避免残留）
echo "➡ Cleaning old files..."
sudo rm -rf "$INSTALL_DIR"/*

# 7. 拷贝文件
echo "➡ Copying plugin files..."
sudo cp -a "$PLUGIN_SRC"/. "$INSTALL_DIR"/

# 8. 权限
echo "➡ Fixing permissions..."
sudo chown -R jellyfin:jellyfin "$INSTALL_DIR"
sudo chmod -R 755 "$INSTALL_DIR"

# 9. 重启 Jellyfin
echo "➡ Restarting Jellyfin..."
sudo systemctl restart jellyfin

echo "=========================================="
echo " ✅ Deploy completed"
echo " Plugin installed at: $INSTALL_DIR"
echo "=========================================="
