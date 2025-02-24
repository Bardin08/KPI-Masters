#!/bin/bash
if [ "$#" -ne 1 ]; then
  echo "Usage: $0 <host_ip>"
  exit 1
fi

host_ip="$1"
max_attempts=10
attempt=1
scanned_key=""

while [ $attempt -le $max_attempts ]; do
  echo "Attempt $attempt: Scanning host key for $host_ip..."
  scanned_key=$(ssh-keyscan -t rsa "$host_ip" 2>/dev/null)
  if [ -n "$scanned_key" ]; then
    break
  fi
  echo "Key not found. Waiting 5 seconds..."
  sleep 5
  attempt=$((attempt + 1))
done

if [ -z "$scanned_key" ]; then
  echo "Error: Could not retrieve host key from $host_ip after $max_attempts attempts."
  exit 1
fi

# Ensure ~/.ssh exists
mkdir -p ~/.ssh

# Compute fingerprint for logging (optional)
scanned_fingerprint=$(echo "$scanned_key" | ssh-keygen -lf - | awk '{print $2}')
echo "Scanned fingerprint: ${scanned_fingerprint}"

echo "$scanned_key" >> ~/.ssh/known_hosts
