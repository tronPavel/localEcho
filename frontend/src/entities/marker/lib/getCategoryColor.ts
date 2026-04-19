export const getCategoryColor = (category: string) => {
    switch (category) {
        case 'Issue': return '#ef4444';
        case 'Event': return '#10b981';
        case 'Announcement': return '#3b82f6';
        case 'Suggestion': return '#8b5cf6';
        case 'Project': return '#f59e0b';
        default: return '#9ca3af';
    }
};