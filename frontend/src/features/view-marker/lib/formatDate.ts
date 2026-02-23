export const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleString('ru-RU', { dateStyle: 'short', timeStyle: 'short' });
};