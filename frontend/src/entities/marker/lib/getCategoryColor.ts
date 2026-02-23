export const getCategoryColor = (category: string) => {
    switch (category) {
        case 'Issue':
            return '#ff0000';
        case 'Event':
            return '#00ff00';
        case 'Announcement':
            return '#0000ff';
        default:
            return '#808080';
    }
};