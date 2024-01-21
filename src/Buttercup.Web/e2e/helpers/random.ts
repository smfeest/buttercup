import { getRandomValues } from 'crypto';

export const randomString = () => {
  const buffer = new Uint32Array(1);
  getRandomValues(buffer);
  return buffer[0].toString(36);
};
