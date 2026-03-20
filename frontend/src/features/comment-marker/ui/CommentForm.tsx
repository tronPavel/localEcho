import { useForm, type SubmitHandler } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { addComment } from '../model/commentApi';
import styles from './CommentForm.module.css';
import { Textarea } from "../../../shared/ui/Textarea/Textarea.tsx";
import { Button } from "../../../shared/ui/Button/Button.tsx";

interface FormData {
    text: string;
}

interface CommentFormProps {
    markerId: string;
}

export const CommentForm = ({ markerId }: CommentFormProps) => {
    const queryClient = useQueryClient();
    const { register, handleSubmit, reset } = useForm<FormData>();
    const mutation = useMutation<void, Error, FormData>({
        mutationFn: (data: FormData) => addComment(markerId, data.text),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['comments', markerId] });
            reset();
        },
    });

    const onSubmit: SubmitHandler<FormData> = (data) => mutation.mutate(data);

    return (
        <form onSubmit={handleSubmit(onSubmit)} className={styles.form}>
            <Textarea placeholder="Ваш комментарий..." {...register('text', { required: true })} />
            <Button type="submit" disabled={mutation.isPending || !markerId}>Отправить</Button>
        </form>
    );
};