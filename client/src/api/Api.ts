/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export enum BookSort {
  Title = 0,
  Price = 1,
  Published = 2,
  Created = 3,
}

export enum Genre {
  Fiction = 0,
  NonFiction = 1,
  SciFi = 2,
  Fantasy = 3,
  Mystery = 4,
  Biography = 5,
}

export enum AuthorSort {
  Name = 0,
  BirthDate = 1,
  Created = 2,
}

export interface AuthorResponse {
  id: string;
  firstName: string;
  lastName: string;
  bio?: string | null;
  nationality?: string | null;
  website?: string | null;
  /** @format date */
  birthDate?: string | null;
  /** @format date-time */
  createdAtUtc: string;
}

export interface AuthorCreateRequest {
  firstName: string;
  lastName: string;
  bio?: string | null;
  nationality?: string | null;
  website?: string | null;
  /** @format date */
  birthDate?: string | null;
}

export interface AuthorUpdateRequest {
  id: string;
  firstName?: string | null;
  lastName?: string | null;
  bio?: string | null;
  nationality?: string | null;
  website?: string | null;
  /** @format date */
  birthDate?: string | null;
}

export interface AuthorReplaceRequest {
  id: string;
  firstName: string;
  lastName: string;
  bio?: string | null;
  nationality?: string | null;
  website?: string | null;
  /** @format date */
  birthDate?: string | null;
}

export interface BookResponse {
  id: string;
  title: string;
  isbn?: string | null;
  genre: Genre;
  /** @format decimal */
  priceDkk: number;
  isOutOfPrint: boolean;
  /** @format date */
  publishedDate?: string | null;
  /** @format date-time */
  createdAtUtc: string;
}

export interface BookCreateRequest {
  title: string;
  isbn?: string | null;
  genre: Genre;
  /** @format decimal */
  priceDkk: number;
  /** @format date */
  publishedDate?: string | null;
}

export interface BookUpdateRequest {
  id: string;
  title?: string | null;
  isbn?: string | null;
  genre?: Genre | null;
  /** @format decimal */
  priceDkk?: number | null;
  isOutOfPrint?: boolean | null;
  /** @format date */
  publishedDate?: string | null;
}

export interface BookReplaceRequest {
  id: string;
  title: string;
  isbn?: string | null;
  genre: Genre;
  /** @format decimal */
  priceDkk: number;
  isOutOfPrint: boolean;
  /** @format date */
  publishedDate?: string | null;
}

export type BookWithAuthorsResponse = BookResponse & {
  authors: AuthorResponse[];
};

export type AuthorWithBooksResponse = AuthorResponse & {
  books: BookResponse[];
};

export interface AuthorBooksLinkParams {
  authorId?: string;
  bookId?: string;
}

export interface AuthorBooksUnlinkParams {
  authorId?: string;
  bookId?: string;
}

export interface AuthorBooksIsLinkedParams {
  authorId?: string;
  bookId?: string;
}

export interface AuthorsDeleteParams {
  id?: string;
}

export interface AuthorsGetByIdParams {
  id?: string;
}

export interface AuthorsSearchParams {
  q?: string;
}

export interface AuthorsGetPageParams {
  /**
   * @format int32
   * @default 1
   */
  page?: number;
  /**
   * @format int32
   * @default 10
   */
  size?: number;
}

export interface AuthorsGetSortedParams {
  by?: AuthorSort;
  /** @default false */
  descending?: boolean;
}

export interface AuthorsGetFilteredParams {
  q?: string | null;
  nationality?: string | null;
  /** @format date */
  bornAfter?: string | null;
  /** @format date */
  bornBefore?: string | null;
}

export interface AuthorsGetForBookParams {
  bookId?: string;
}

export interface BooksDeleteParams {
  id?: string;
}

export interface BooksMarkOutOfPrintParams {
  id?: string;
}

export interface BooksMarkInPrintParams {
  id?: string;
}

export interface BooksGetByIdParams {
  id?: string;
}

export interface BooksSearchParams {
  q?: string;
}

export interface BooksGetPageParams {
  /**
   * @format int32
   * @default 1
   */
  page?: number;
  /**
   * @format int32
   * @default 10
   */
  size?: number;
}

export interface BooksGetSortedParams {
  by?: BookSort;
  /** @default false */
  descending?: boolean;
}

export interface BooksGetFilteredParams {
  q?: string | null;
  genre?: Genre | null;
  outOfPrint?: boolean | null;
  /** @format decimal */
  minPrice?: number | null;
  /** @format decimal */
  maxPrice?: number | null;
}

export interface BooksGetByAuthorParams {
  authorId?: string;
}

export interface LibraryQueriesGetBooksWithAuthorsParams {
  /**
   * @format int32
   * @default 1
   */
  page?: number;
  /**
   * @format int32
   * @default 10
   */
  size?: number;
}

export interface LibraryQueriesGetBookWithAuthorsParams {
  id?: string;
}

export interface LibraryQueriesSearchBooksByAuthorParams {
  q?: string;
}

export interface LibraryQueriesGetAuthorsWithBooksParams {
  /**
   * @format int32
   * @default 1
   */
  page?: number;
  /**
   * @format int32
   * @default 10
   */
  size?: number;
}

export interface LibraryQueriesGetAuthorWithBooksParams {
  id?: string;
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown>
  extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "http://localhost:5234";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) =>
    fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter(
      (key) => "undefined" !== typeof query[key],
    );
    return keys
      .map((key) =>
        Array.isArray(query[key])
          ? this.addArrayQueryParam(query, key)
          : this.addQueryParam(query, key),
      )
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.JsonApi]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.Text]: (input: any) =>
      input !== null && typeof input !== "string"
        ? JSON.stringify(input)
        : input,
    [ContentType.FormData]: (input: any) => {
      if (input instanceof FormData) {
        return input;
      }

      return Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
              ? JSON.stringify(property)
              : `${property}`,
        );
        return formData;
      }, new FormData());
    },
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(
    params1: RequestParams,
    params2?: RequestParams,
  ): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (
    cancelToken: CancelToken,
  ): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<T> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(
      `${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`,
      {
        ...requestParams,
        headers: {
          ...(requestParams.headers || {}),
          ...(type && type !== ContentType.FormData
            ? { "Content-Type": type }
            : {}),
        },
        signal:
          (cancelToken
            ? this.createAbortSignal(cancelToken)
            : requestParams.signal) || null,
        body:
          typeof body === "undefined" || body === null
            ? null
            : payloadFormatter(body),
      },
    ).then(async (response) => {
      const r = response as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const responseToParse = responseFormat ? response.clone() : response;
      const data = !responseFormat
        ? r
        : await responseToParse[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data.data;
    });
  };
}

/**
 * @title My Title
 * @version 1.0.0
 * @baseUrl http://localhost:5234
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  authorBooks = {
    /**
     * No description
     *
     * @tags AuthorBooks
     * @name AuthorBooksLink
     * @request POST:/AuthorBooks/Link
     */
    authorBooksLink: (
      query: AuthorBooksLinkParams = {},
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/AuthorBooks/Link`,
        method: "POST",
        query: query,
        ...params,
      }),

    /**
     * No description
     *
     * @tags AuthorBooks
     * @name AuthorBooksUnlink
     * @request DELETE:/AuthorBooks/Unlink
     */
    authorBooksUnlink: (
      query: AuthorBooksUnlinkParams = {},
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/AuthorBooks/Unlink`,
        method: "DELETE",
        query: query,
        ...params,
      }),

    /**
     * No description
     *
     * @tags AuthorBooks
     * @name AuthorBooksIsLinked
     * @request GET:/AuthorBooks/IsLinked
     */
    authorBooksIsLinked: (
      query: AuthorBooksIsLinkedParams = {},
      params: RequestParams = {},
    ) =>
      this.request<boolean, any>({
        path: `/AuthorBooks/IsLinked`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  authors = {
    /**
     * No description
     *
     * @tags Authors
     * @name AuthorsCreate
     * @request POST:/Authors/Create
     */
    authorsCreate: (data: AuthorCreateRequest, params: RequestParams = {}) =>
      this.request<AuthorResponse, any>({
        path: `/Authors/Create`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Authors
     * @name AuthorsUpdate
     * @request PATCH:/Authors/Update
     */
    authorsUpdate: (data: AuthorUpdateRequest, params: RequestParams = {}) =>
      this.request<AuthorResponse, any>({
        path: `/Authors/Update`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Authors
     * @name AuthorsReplace
     * @request PUT:/Authors/Replace
     */
    authorsReplace: (data: AuthorReplaceRequest, params: RequestParams = {}) =>
      this.request<AuthorResponse, any>({
        path: `/Authors/Replace`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Authors
     * @name AuthorsDelete
     * @request DELETE:/Authors/Delete
     */
    authorsDelete: (
      query: AuthorsDeleteParams = {},
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/Authors/Delete`,
        method: "DELETE",
        query: query,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Authors
     * @name AuthorsGetAll
     * @request GET:/Authors/GetAll
     */
    authorsGetAll: (params: RequestParams = {}) =>
      this.request<AuthorResponse[], any>({
        path: `/Authors/GetAll`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Authors
     * @name AuthorsGetById
     * @request GET:/Authors/GetById
     */
    authorsGetById: (
      query: AuthorsGetByIdParams = {},
      params: RequestParams = {},
    ) =>
      this.request<AuthorResponse, any>({
        path: `/Authors/GetById`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Authors
     * @name AuthorsCount
     * @request GET:/Authors/Count
     */
    authorsCount: (params: RequestParams = {}) =>
      this.request<number, any>({
        path: `/Authors/Count`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Authors
     * @name AuthorsSearch
     * @request GET:/Authors/Search
     */
    authorsSearch: (
      query: AuthorsSearchParams = {},
      params: RequestParams = {},
    ) =>
      this.request<AuthorResponse[], any>({
        path: `/Authors/Search`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Authors
     * @name AuthorsGetPage
     * @request GET:/Authors/GetPage
     */
    authorsGetPage: (
      query: AuthorsGetPageParams = {},
      params: RequestParams = {},
    ) =>
      this.request<AuthorResponse[], any>({
        path: `/Authors/GetPage`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Authors
     * @name AuthorsGetSorted
     * @request GET:/Authors/GetSorted
     */
    authorsGetSorted: (
      query: AuthorsGetSortedParams = {},
      params: RequestParams = {},
    ) =>
      this.request<AuthorResponse[], any>({
        path: `/Authors/GetSorted`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Authors
     * @name AuthorsGetFiltered
     * @request GET:/Authors/GetFiltered
     */
    authorsGetFiltered: (
      query: AuthorsGetFilteredParams = {},
      params: RequestParams = {},
    ) =>
      this.request<AuthorResponse[], any>({
        path: `/Authors/GetFiltered`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Authors
     * @name AuthorsGetForBook
     * @request GET:/Authors/GetForBook
     */
    authorsGetForBook: (
      query: AuthorsGetForBookParams = {},
      params: RequestParams = {},
    ) =>
      this.request<AuthorResponse[], any>({
        path: `/Authors/GetForBook`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Authors
     * @name AuthorsGetWithoutBooks
     * @request GET:/Authors/GetWithoutBooks
     */
    authorsGetWithoutBooks: (params: RequestParams = {}) =>
      this.request<AuthorResponse[], any>({
        path: `/Authors/GetWithoutBooks`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  books = {
    /**
     * No description
     *
     * @tags Books
     * @name BooksCreate
     * @request POST:/Books/Create
     */
    booksCreate: (data: BookCreateRequest, params: RequestParams = {}) =>
      this.request<BookResponse, any>({
        path: `/Books/Create`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksUpdate
     * @request PATCH:/Books/Update
     */
    booksUpdate: (data: BookUpdateRequest, params: RequestParams = {}) =>
      this.request<BookResponse, any>({
        path: `/Books/Update`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksReplace
     * @request PUT:/Books/Replace
     */
    booksReplace: (data: BookReplaceRequest, params: RequestParams = {}) =>
      this.request<BookResponse, any>({
        path: `/Books/Replace`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksDelete
     * @request DELETE:/Books/Delete
     */
    booksDelete: (query: BooksDeleteParams = {}, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/Books/Delete`,
        method: "DELETE",
        query: query,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksMarkOutOfPrint
     * @request POST:/Books/MarkOutOfPrint
     */
    booksMarkOutOfPrint: (
      query: BooksMarkOutOfPrintParams = {},
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/Books/MarkOutOfPrint`,
        method: "POST",
        query: query,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksMarkInPrint
     * @request POST:/Books/MarkInPrint
     */
    booksMarkInPrint: (
      query: BooksMarkInPrintParams = {},
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/Books/MarkInPrint`,
        method: "POST",
        query: query,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksGetAll
     * @request GET:/Books/GetAll
     */
    booksGetAll: (params: RequestParams = {}) =>
      this.request<BookResponse[], any>({
        path: `/Books/GetAll`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksGetById
     * @request GET:/Books/GetById
     */
    booksGetById: (
      query: BooksGetByIdParams = {},
      params: RequestParams = {},
    ) =>
      this.request<BookResponse, any>({
        path: `/Books/GetById`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksCount
     * @request GET:/Books/Count
     */
    booksCount: (params: RequestParams = {}) =>
      this.request<number, any>({
        path: `/Books/Count`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksSearch
     * @request GET:/Books/Search
     */
    booksSearch: (query: BooksSearchParams = {}, params: RequestParams = {}) =>
      this.request<BookResponse[], any>({
        path: `/Books/Search`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksGetPage
     * @request GET:/Books/GetPage
     */
    booksGetPage: (
      query: BooksGetPageParams = {},
      params: RequestParams = {},
    ) =>
      this.request<BookResponse[], any>({
        path: `/Books/GetPage`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksGetSorted
     * @request GET:/Books/GetSorted
     */
    booksGetSorted: (
      query: BooksGetSortedParams = {},
      params: RequestParams = {},
    ) =>
      this.request<BookResponse[], any>({
        path: `/Books/GetSorted`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksGetFiltered
     * @request GET:/Books/GetFiltered
     */
    booksGetFiltered: (
      query: BooksGetFilteredParams = {},
      params: RequestParams = {},
    ) =>
      this.request<BookResponse[], any>({
        path: `/Books/GetFiltered`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksGetByAuthor
     * @request GET:/Books/GetByAuthor
     */
    booksGetByAuthor: (
      query: BooksGetByAuthorParams = {},
      params: RequestParams = {},
    ) =>
      this.request<BookResponse[], any>({
        path: `/Books/GetByAuthor`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksGetWithoutAuthors
     * @request GET:/Books/GetWithoutAuthors
     */
    booksGetWithoutAuthors: (params: RequestParams = {}) =>
      this.request<BookResponse[], any>({
        path: `/Books/GetWithoutAuthors`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Books
     * @name BooksGetAveragePrice
     * @request GET:/Books/GetAveragePrice
     */
    booksGetAveragePrice: (params: RequestParams = {}) =>
      this.request<number, any>({
        path: `/Books/GetAveragePrice`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  libraryQueries = {
    /**
     * No description
     *
     * @tags LibraryQueries
     * @name LibraryQueriesGetBooksWithAuthors
     * @request GET:/LibraryQueries/GetBooksWithAuthors
     */
    libraryQueriesGetBooksWithAuthors: (
      query: LibraryQueriesGetBooksWithAuthorsParams = {},
      params: RequestParams = {},
    ) =>
      this.request<BookWithAuthorsResponse[], any>({
        path: `/LibraryQueries/GetBooksWithAuthors`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags LibraryQueries
     * @name LibraryQueriesGetBookWithAuthors
     * @request GET:/LibraryQueries/GetBookWithAuthors
     */
    libraryQueriesGetBookWithAuthors: (
      query: LibraryQueriesGetBookWithAuthorsParams = {},
      params: RequestParams = {},
    ) =>
      this.request<BookWithAuthorsResponse, any>({
        path: `/LibraryQueries/GetBookWithAuthors`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags LibraryQueries
     * @name LibraryQueriesSearchBooksByAuthor
     * @request GET:/LibraryQueries/SearchBooksByAuthor
     */
    libraryQueriesSearchBooksByAuthor: (
      query: LibraryQueriesSearchBooksByAuthorParams = {},
      params: RequestParams = {},
    ) =>
      this.request<BookWithAuthorsResponse[], any>({
        path: `/LibraryQueries/SearchBooksByAuthor`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags LibraryQueries
     * @name LibraryQueriesGetAuthorsWithBooks
     * @request GET:/LibraryQueries/GetAuthorsWithBooks
     */
    libraryQueriesGetAuthorsWithBooks: (
      query: LibraryQueriesGetAuthorsWithBooksParams = {},
      params: RequestParams = {},
    ) =>
      this.request<AuthorWithBooksResponse[], any>({
        path: `/LibraryQueries/GetAuthorsWithBooks`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags LibraryQueries
     * @name LibraryQueriesGetAuthorWithBooks
     * @request GET:/LibraryQueries/GetAuthorWithBooks
     */
    libraryQueriesGetAuthorWithBooks: (
      query: LibraryQueriesGetAuthorWithBooksParams = {},
      params: RequestParams = {},
    ) =>
      this.request<AuthorWithBooksResponse, any>({
        path: `/LibraryQueries/GetAuthorWithBooks`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
}
