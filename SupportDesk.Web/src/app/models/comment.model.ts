export interface Comment {
  id: number;
  authorName: string;
  body: string;
  createdAt: string;
}

export interface CommentCreateRequest {
  authorName: string;
  body: string;
}
