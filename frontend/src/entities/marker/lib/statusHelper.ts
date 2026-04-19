import type { MarkerCategory } from '../model/types';

export const CATEGORY_STATUSES: Record<MarkerCategory, string[]> = {
    Issue: ['Active', 'InProgress', 'Resolved'],
    Event: ['Upcoming', 'Ongoing', 'Passed'],
    Announcement: ['Current', 'Archived'],
    Suggestion: ['Review', 'Accepted', 'Rejected'],
    Project: ['Active', 'InProgress', 'Resolved']
};

export const getStatusLabel = (status: string) => {
    const labels: Record<string, string> = {
        Active: 'Активно', InProgress: 'В работе', Resolved: 'Решено',
        Upcoming: 'Ожидается', Ongoing: 'Идет сейчас', Passed: 'Завершено',
        Current: 'Актуально', Archived: 'В архиве',
        Review: 'На рассмотрении', Accepted: 'Принято', Rejected: 'Отклонено'
    };
    return labels[status] || status;
};