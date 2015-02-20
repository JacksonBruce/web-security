/*
 * Copyright (c) 2013, Kristian Rosenvold
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 * Neither the name of OWASP nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

package org.owasp.validator.html.test;

import org.owasp.validator.html.InternalPolicy;
import org.owasp.validator.html.Policy;
import org.owasp.validator.html.PolicyException;
import org.owasp.validator.html.model.Tag;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.xml.sax.SAXException;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.net.URI;
import java.net.URL;
import java.util.Collections;
import java.util.HashMap;
import java.util.Map;

/**
 * @author Kristian Rosenvold
 */
public class TestPolicy extends InternalPolicy {

    protected TestPolicy(URL baseUrl, Policy.ParseContext parseContext) throws PolicyException {
        super(baseUrl, parseContext);
    }

    protected TestPolicy(Policy old, Map<String, String> directives, Map<String, Tag> tagRules) {
        super(old, directives, tagRules);
    }

    public static TestPolicy getInstance() throws PolicyException {
        return getInstance(DEFAULT_POLICY_URI);
    }

    public static TestPolicy getInstance(String filename) throws PolicyException {
        File file = new File(filename);
        return getInstance(file);
    }

    public static TestPolicy getInstance(File file) throws PolicyException {
        try {
            URI uri = file.toURI();
            return getInstance(uri.toURL());
        } catch (IOException e) {
            throw new PolicyException(e);
        }
    }

    public static TestPolicy getInstance(URL url) throws PolicyException {
        return new TestPolicy(url, getParseContext(getTopLevelElement(url), url));
    }

    public TestPolicy cloneWithDirective(String name, String value) {
        Map<String, String> directives = new HashMap<String, String>(this.directives);
        directives.put(name, value);
        return new TestPolicy(this, Collections.unmodifiableMap(directives), tagRules);
    }

    public TestPolicy addTagRule(Tag tag) {
        Map<String, Tag> newTagRules = new HashMap<String, Tag>(tagRules);
        newTagRules.put(tag.getName().toLowerCase(), tag);
        return new TestPolicy(this, this.directives, newTagRules);

    }

    public TestPolicy mutateTag(Tag tag) {
        Map<String, Tag> newRUles = new HashMap<String, Tag>(this.tagRules);
        newRUles.put( tag.getName().toLowerCase(), tag);
        return new TestPolicy(this, this.directives, newRUles);
    }

}
