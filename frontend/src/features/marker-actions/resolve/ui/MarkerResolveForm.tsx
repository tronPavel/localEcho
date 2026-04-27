import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useResolveMarker } from '../model/useResolveMarker';
import { CATEGORY_STATUSES, getStatusLabel } from '@/entities/marker/lib/statusHelper';

import { Button } from '@/shared/ui/Button/Button';
import { Textarea } from '@/shared/ui/Textarea/Textarea';
import { Select } from '@/shared/ui/Select/Select';
import { ImageUploader } from '@/shared/ui/ImageUploader/ImageUploader';

import styles from './MarkerResolveForm.module.css';
import {markerApi} from "@/entities/marker";
import {useQuery} from "@tanstack/react-query";

interface MarkerResolveFormProps {
    markerId: string;
    onSuccess: () => void;
    onCancel: () => void;
}

interface FormFields {
    newStatus: string;
    comment: string;
}

export const MarkerResolveForm = ({ markerId, onSuccess, onCancel }: MarkerResolveFormProps) => {
    const [files, setFiles] = useState<File[]>([]);
    const { mutate, isPending } = useResolveMarker(markerId);
    const { data: marker, isLoading } = useQuery({
        queryKey: ['marker', markerId],
        queryFn: () => markerApi.getDetails(markerId),
    });

    if (isLoading) return <div>Подготовка данных...</div>;
    if (!marker) return null;

    const availableStatuses = CATEGORY_STATUSES[marker.category] || [];

    const { register, handleSubmit, formState: { errors } } = useForm<FormFields>({
        defaultValues: {
            newStatus: marker.status,
            comment: ''
        }
    });

    const onSubmit = (data: FormFields) => {
        mutate({
            newStatus: data.newStatus,
            comment: data.comment,
            imageFiles: files
        }, {
            onSuccess: () => onSuccess()
        });
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className={styles.form}>
            <div className={styles.officialHeader}>
                <h3>Действия официального представителя</h3>
                <p>Выберите стадию решения и оставьте публичное пояснение.</p>
            </div>

            <div className={styles.controlsGrid}>
                <Select
                    label="Новый статус"
                    {...register('newStatus', { required: true })}
                >
                    {availableStatuses.map(s => (
                        <option key={s} value={s}>{getStatusLabel(s)}</option>
                    ))}
                </Select>

                <Textarea
                    label="Комментарий представителя"
                    placeholder="Напр: Заявка принята в работу, срок исполнения — 3 дня..."
                    {...register('comment', { required: 'Текст комментария обязателен для истории' })}
                    error={errors.comment?.message}
                />
            </div>

            <ImageUploader
                label="Фотофиксация изменений"
                onFilesChange={setFiles}
                maxFiles={5}
            />

            <div className={styles.footer}>
                <Button type="submit" disabled={isPending}>
                    {isPending ? 'Публикация...' : '✅ Подтвердить'}
                </Button>
                <Button variant="outline" type="button" onClick={onCancel}>
                    Отмена
                </Button>
            </div>
        </form>
    );
};