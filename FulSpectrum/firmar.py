import hmac, hashlib

secret = b"dev_stripe_webhook_secret"

with open("payload.json", "rb") as f:
    body = f.read()

print(hmac.new(secret, body, hashlib.sha256).hexdigest().upper())
