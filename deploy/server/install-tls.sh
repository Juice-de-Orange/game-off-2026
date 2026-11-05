#!/bin/bash
# One-time setup of kestrel9.maxlworks.com on Geoffrey (P49, docs/DEPLOY.md): certificate, nginx site, one line in the SNI router.
# Run as max from /opt/kestrel9 next to nginx-site.conf. Safe to run again. Restores the router if the nginx config test fails.
set -euo pipefail

CONF=/etc/nginx/sites-available/kestrel9
ROUTER=/etc/nginx/modules-enabled/60-stream-pete.conf
BACKUP=/root/60-stream-pete.conf.bak-kestrel9
HERE=$(cd "$(dirname "$0")" && pwd)

status() {
  for url in https://maxlworks.com https://jobsuchomat.oberrauch-salzburg.com; do
    curl -s -o /dev/null -w "$url %{http_code}\n" --max-time 10 "$url"
  done
}

echo "--- before"; status
sudo -n test -f "$BACKUP" || sudo -n cp -p "$ROUTER" "$BACKUP"

if ! sudo -n test -f /etc/letsencrypt/live/kestrel9.maxlworks.com/fullchain.pem; then
  # Port 80 only, so the webroot challenge works before a certificate exists
  sudo -n tee "$CONF" >/dev/null <<'NGINX'
server {
    listen 80;
    listen [::]:80;
    server_name kestrel9.maxlworks.com;
    location /.well-known/acme-challenge/ { root /var/www/certbot; }
    location / { return 404; }
}
NGINX
  sudo -n ln -sf "$CONF" /etc/nginx/sites-enabled/kestrel9
  sudo -n nginx -t
  sudo -n systemctl reload nginx
  sudo -n certbot certonly --webroot -w /var/www/certbot -d kestrel9.maxlworks.com --non-interactive --keep-until-expiring
fi

sed 's/\r$//' "$HERE/nginx-site.conf" | sudo -n tee "$CONF" >/dev/null
sudo -n ln -sf "$CONF" /etc/nginx/sites-enabled/kestrel9
if ! sudo -n grep -q 'kestrel9.maxlworks.com' "$ROUTER"; then
  sudo -n sed -i 's|^\(\s*\)www\.maxlworks\.com\s\+127\.0\.0\.1:8446;|&\n\n\1# kestrel9.maxlworks.com (the game, P49 game-off-2026): own TLS on a private port.\n\1kestrel9.maxlworks.com   127.0.0.1:8447;|' "$ROUTER"
fi
sudo -n grep -n 'kestrel9' "$ROUTER"

if ! sudo -n nginx -t; then
  sudo -n cp -p "$BACKUP" "$ROUTER"
  sudo -n rm -f /etc/nginx/sites-enabled/kestrel9
  sudo -n nginx -t
  sudo -n systemctl reload nginx
  echo "RESTORED: the config test failed, nothing changed for the other sites." >&2
  exit 1
fi
sudo -n systemctl reload nginx
sleep 2

echo "--- after"; status
echo "--- game"
curl -s --max-time 15 https://kestrel9.maxlworks.com/version.txt
curl -sI --max-time 15 https://kestrel9.maxlworks.com/ | grep -i -E '^(HTTP|strict-transport)'
echo "--- certificate renewal"
systemctl is-active certbot.timer
sudo -n ls /etc/letsencrypt/renewal-hooks/deploy
