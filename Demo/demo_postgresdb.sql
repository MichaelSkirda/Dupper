--
-- PostgreSQL database dump
--

-- Dumped from database version 16.0 (Ubuntu 16.0-1.pgdg20.04+1)
-- Dumped by pg_dump version 16.0 (Ubuntu 16.0-1.pgdg20.04+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: comments; Type: TABLE; Schema: public; Owner: mirtuser
--

CREATE TABLE public.comments (
    id integer NOT NULL,
    user_id integer,
    text character varying
);


ALTER TABLE public.comments OWNER TO mirtuser;

--
-- Name: comments_id_seq; Type: SEQUENCE; Schema: public; Owner: mirtuser
--

ALTER TABLE public.comments ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.comments_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: users; Type: TABLE; Schema: public; Owner: mirtuser
--

CREATE TABLE public.users (
    id integer NOT NULL,
    name character varying
);


ALTER TABLE public.users OWNER TO mirtuser;

--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: mirtuser
--

ALTER TABLE public.users ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Data for Name: comments; Type: TABLE DATA; Schema: public; Owner: mirtuser
--

COPY public.comments (id, user_id, text) FROM stdin;
1	1	Hello, Alice!
2	1	Hello, Bob!
3	1	How are you?
4	2	Ok.
5	3	A lot of job.
6	3	Sorry, can't talk
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: mirtuser
--

COPY public.users (id, name) FROM stdin;
1	Michael
2	Alice
3	Bob
\.


--
-- Name: comments_id_seq; Type: SEQUENCE SET; Schema: public; Owner: mirtuser
--

SELECT pg_catalog.setval('public.comments_id_seq', 6, true);


--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: mirtuser
--

SELECT pg_catalog.setval('public.users_id_seq', 3, true);


--
-- Name: comments comments_pkey; Type: CONSTRAINT; Schema: public; Owner: mirtuser
--

ALTER TABLE ONLY public.comments
    ADD CONSTRAINT comments_pkey PRIMARY KEY (id);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: mirtuser
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: comments comments_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: mirtuser
--

ALTER TABLE ONLY public.comments
    ADD CONSTRAINT comments_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- PostgreSQL database dump complete
--

