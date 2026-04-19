import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';

import { createMarker } from '../model/createMarkerApi';
import { useMapInteractionStore } from '../model/interactionStore';
import { Button } from '@/shared/ui/Button/Button';
import { Input } from '@/shared/ui/Input/Input';
import { Textarea } from '@/shared/ui/Textarea/Textarea';
import { Select } from '@/shared/ui/Select/Select';

import styles from './CreateMarkerForm.module.css';
import type {MarkerCategory} from "@/entities/marker/model/types.ts";
import {ImageUploader} from "@/shared/ui/ImageUploader/ImageUploader.tsx";

interface CreateMarkerFields {
    title: string;
    description: string;
    category: MarkerCategory;
    scheduledAt?: string;
}
export const CreateMarkerForm = ({ onSuccess }: { onSuccess: () => void }) => {
    const { tempPoints, clear } = useMapInteractionStore();
    const queryClient = useQueryClient();

    const [files, setFiles] = useState<File[]>([]);

    const { register, handleSubmit, watch, formState: { errors } } = useForm<CreateMarkerFields>({
        defaultValues: {
            category: 'Issue',
            title: '',
            description: ''
        }
    });

    const currentCategory = watch('category');

    const mutation = useMutation({
        mutationFn: (data: any) => createMarker({
            ...data,
            points: tempPoints,
            imageFiles: files
        }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['markers'] });
            toast.success("Метка успешно опубликована!");
            clear();
            onSuccess();
        },
        onError: (err: any) => toast.error(err.response?.data?.detail || "Ошибка при создании")
    });



    return (
        <form onSubmit={handleSubmit(d => mutation.mutate(d))} className={styles.form}>
            <div className={styles.formScrollArea}>
                <Input
                    label="Что происходит?"
                    placeholder="Напр: Разбитая лавочка в сквере..."
                    {...register('title', { required: "Назовите событие" })}
                    error={errors.title?.message as string}
                />

                <Textarea
                    label="Детали"
                    placeholder="Опишите ситуацию подробнее для соседей и служб..."
                    {...register('description')}
                />

                <Select label="Категория" {...register('category')}>
                    <option value="Issue">⚠️ Проблема ЖКХ</option>
                    <option value="Event">🎉 Мероприятие</option>
                    <option value="Announcement">📢 Объявление</option>
                    <option value="Suggestion">💡 Предложение</option>
                    <option value="Project">🏗 Проект города</option>
                </Select>

                {currentCategory === 'Event' && (
                    <Input
                        label="Дата и время"
                        type="datetime-local"
                        {...register('scheduledAt', { required: "Укажите дату начала" })}
                        error={errors.scheduledAt?.message as string}
                    />
                )}

                <ImageUploader onFilesChange={(f) => setFiles(f)} />
            </div>

            <div className={styles.stickyActions}>
                <Button
                    variant="outline"
                    type="button"
                    onClick={() => { clear(); onSuccess(); }}
                >
                    Отмена
                </Button>
                <Button type="submit" disabled={mutation.isPending}>
                    {mutation.isPending ? 'Опубликовываем...' : 'Опубликовать'}
                </Button>
            </div>
        </form>
    );
};