const mailpitServer = process.env.MAILPIT_SERVER ?? 'http://localhost:8025';

/**
 * Gets the text content of the latest email sent to a particular email address.
 *
 * @param to The email address.
 */
export const latestEmailText = async (to: string): Promise<string> => {
  const response = await fetch(
    `${mailpitServer}/view/latest.txt?query=to:${encodeURIComponent(to)}`,
  );

  return await response.text();
};
