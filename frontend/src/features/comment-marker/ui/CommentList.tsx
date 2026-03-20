import { useQuery } from '@tanstack/react-query';
import { formatDate } from '../../view-marker/lib/formatDate.ts';
import styles from './CommentList.module.css';
import { UserAvatar } from "../../../entities/user/ui/UserAvatar.tsx";
import { getComments, type CommentDto } from "../model/commentApi.ts"; // Assume CommentDto has user: Partial<UserDto>
import type { UserDto } from '@/entities/user/model/types.ts'; // For UserAvatar

interface CommentListProps {
    markerId: string;
}

export const CommentList = ({ markerId }: CommentListProps) => {
    const { data: comments = [] } = useQuery<CommentDto[]>({
        queryKey: ['comments', markerId],
        queryFn: () => getComments(markerId),
    });

    return (
        <div className={styles.list}>
            {comments.map((comment) => (
                <div key={comment.id} className={styles.comment}>
                    <UserAvatar user={{ ...comment.user, email: '', points: 0, roles: [] } as UserDto} size="small" />
                    <div>
                        <strong>{comment.user.name}</strong>
                        <p>{comment.text}</p>
                        <small>{formatDate(comment.createdAt)}</small>
                    </div>
                </div>
            ))}
        </div>
    );
};