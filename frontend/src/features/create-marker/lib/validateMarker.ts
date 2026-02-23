import { z } from 'zod';

export const createMarkerSchema = z.object({
    title: z.string().min(1, 'Обязательное поле'),
    description: z.string().optional(),
    category: z.enum(['Issue', 'Event', 'Announcement']),
});