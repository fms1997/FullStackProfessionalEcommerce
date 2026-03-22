const SAFE_ID_PATTERN = /^[a-zA-Z0-9_-]{1,80}$/;

export const sanitizeText = (value: string, fallback = ""): string => {
  const trimmed = value.trim();
  if (!trimmed) {
    return fallback;
  }

  return Array.from(trimmed).filter((char) => {
    const code = char.charCodeAt(0);
    return code >= 32 && code !== 127;
  }).join("");
};

export const sanitizeIdentifier = (value: string, fallback = ""): string => {
  const trimmed = sanitizeText(value, fallback);
  return SAFE_ID_PATTERN.test(trimmed) ? trimmed : fallback;
};
