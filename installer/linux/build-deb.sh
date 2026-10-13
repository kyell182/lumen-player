#!/usr/bin/env bash
# Bouwt Lumen en maakt een Debian/Ubuntu-pakket (artifacts/installer/lumen_<versie>_amd64.deb).
# Vereist: .NET 8 SDK en dpkg-deb.
set -euo pipefail

VERSION="${1:-1.0.0}"
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
HERE="$ROOT/installer/linux"
PUBLISH="$ROOT/artifacts/publish/linux-x64"
PKG="$ROOT/artifacts/deb/lumen_${VERSION}_amd64"
OUT="$ROOT/artifacts/installer"

echo "==> Publiceren ($VERSION)..."
rm -rf "$PUBLISH"
dotnet publish "$ROOT/src/Lumen/Lumen.csproj" \
  -c Release -r linux-x64 --self-contained true \
  -p:Version="$VERSION" -o "$PUBLISH"

echo "==> Pakketstructuur opbouwen..."
rm -rf "$PKG"
install -d "$PKG/DEBIAN" "$PKG/opt/lumen" "$PKG/usr/bin" \
  "$PKG/usr/share/applications" "$PKG/usr/share/icons/hicolor/256x256/apps" "$OUT"

cp -r "$PUBLISH/." "$PKG/opt/lumen/"
chmod 755 "$PKG/opt/lumen/Lumen"
ln -s /opt/lumen/Lumen "$PKG/usr/bin/lumen"

install -m 644 "$HERE/lumen.desktop" "$PKG/usr/share/applications/lumen.desktop"
install -m 644 "$ROOT/src/Lumen/Assets/lumen-256.png" "$PKG/usr/share/icons/hicolor/256x256/apps/lumen.png"
install -m 755 "$HERE/postinst" "$PKG/DEBIAN/postinst"
install -m 755 "$HERE/postrm" "$PKG/DEBIAN/postrm"

SIZE_KB="$(du -sk "$PKG" | cut -f1)"
sed -e "s/@VERSION@/$VERSION/" -e "s/@SIZE@/$SIZE_KB/" "$HERE/control.in" > "$PKG/DEBIAN/control"

echo "==> .deb maken..."
dpkg-deb --build --root-owner-group "$PKG" "$OUT/lumen_${VERSION}_amd64.deb"
echo "==> Klaar: $OUT/lumen_${VERSION}_amd64.deb"
