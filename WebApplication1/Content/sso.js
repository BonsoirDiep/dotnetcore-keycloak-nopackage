class SSOVinorSoftFE {
    constructor(opt) {
        this.opt = opt;
        this.isNode = 0;
        if (typeof process === 'object' && typeof window === 'undefined') {
            this.isNode = 1;
        }
    }

    Base64() {
        var Base64 = {
            _keyStr: "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=",
            encode: function (a) {
                var d = "", c = 0; for (a = Base64._utf8_encode(a); c < a.length;) { var b = a.charCodeAt(c++); var e = a.charCodeAt(c++); var f = a.charCodeAt(c++); var g = b >> 2; b = (b & 3) << 4 | e >> 4; var h = (e & 15) << 2 | f >> 6; var k = f & 63; isNaN(e) ? h = k = 64 : isNaN(f) && (k = 64); d = d + this._keyStr.charAt(g) + this._keyStr.charAt(b) + this._keyStr.charAt(h) + this._keyStr.charAt(k) } return d
            },
            decode: function (a) {
                var d = "", c = 0; for (a = a.replace(/[^A-Za-z0-9\+\/\=]/g, ""); c < a.length;) {
                    var b = this._keyStr.indexOf(a.charAt(c++)); var e = this._keyStr.indexOf(a.charAt(c++)); var f = this._keyStr.indexOf(a.charAt(c++)); var g = this._keyStr.indexOf(a.charAt(c++)); b = b << 2 | e >> 4; e = (e & 15) << 4 | f >> 2; var h = (f & 3) << 6 | g; d += String.fromCharCode(b); 64 != f && (d += String.fromCharCode(e)); 64 != g && (d += String.fromCharCode(h))
                };
                d = Base64._utf8_decode(d);
                return d.replace(/\0/g, '');
            }, _utf8_encode: function (a) {
                a = a.replace(/\r\n/g, "\n"); for (var d = "", c = 0; c < a.length; c++) {
                    var b = a.charCodeAt(c); 128 > b ? d += String.fromCharCode(b) : (127 <
                        b && 2048 > b ? d += String.fromCharCode(b >> 6 | 192) : (d += String.fromCharCode(b >> 12 | 224), d += String.fromCharCode(b >> 6 & 63 | 128)), d += String.fromCharCode(b & 63 | 128))
                } return d
            }, _utf8_decode: function (a) {
                var d = "", c = 0, c1, c2, c3;
                for (c1 = c2 = 0; c < a.length;) {
                    var b = a.charCodeAt(c); 128 > b ? (d += String.fromCharCode(b), c++) : 191 < b && 224 > b ? (c2 = a.charCodeAt(c + 1), d += String.fromCharCode((b & 31) << 6 | c2 & 63), c += 2) : (c2 = a.charCodeAt(c + 1), c3 = a.charCodeAt(c + 2), d += String.fromCharCode((b & 15) << 12 | (c2 & 63) << 6 | c3 & 63), c += 3)
                }
                return d;
            }
        };

        return Base64;

    }
    tokenInfor(token) {
        var res = {};
        function isExpired() {
            // console.log('isExpired', (res.content.exp * 1000) < Date.now(), (res.content.exp * 1000) - Date.now())
            return ((res.content.exp * 1000) < Date.now())
        }
        try {
            const parts = token.split('.');
            var base64 = this.Base64();

            var tt1 = base64.decode(parts[0]).trim();
            var tt2 = base64.decode(parts[1]).trim();
            var tt3 = base64.decode(parts[2]).trim();

            res.header = JSON.parse(tt1);
            res.content = JSON.parse(tt2);
            if (isExpired()) throw -1;
            res.signature = tt3;
            res.signed = parts[0] + '.' + parts[1];
            res.content.expired = false;
        }
        catch (_ex) {
            console.log(_ex);
            res.content = res.content || {};
            res.content.exp = 0;
            res.content.expired = true;
        }
        return res;
    }

    isExpired(token) {
        var a = this.tokenInfor(token);
        return a.content.expired;
    }

    userInfor(accessToken) {
        this.accessToken = accessToken;
        var a = this.tokenInfor(accessToken);
        if (a.content.expired) this.accessToken = '';
        return a.content;
    }

    getLoginUrl() {
        var config = {
            realmUrl: this.opt.realmUrl,
            clientId: this.opt.clientId,
            callbackUrl: this.opt.callbackUrl
        };
        const callbackUrl = config.callbackUrl;
        function genUUid() {
            const s = []
            const hexDigits = '0123456789abcdef'
            for (let i = 0; i < 36; i++) {
                var x1 = Math.floor(Math.random() * 0x10);
                s[i] = hexDigits.substring(x1, x1 + 1)
            }
            s[14] = '4'
            s[19] = hexDigits.substring((s[19] & 0x3) | 0x8, 1)
            s[8] = s[13] = s[18] = s[23] = '-'
            return s.join('')
        }
        const uuid = genUUid();
        let url = config.realmUrl +
            '/protocol/openid-connect/auth' +
            '?client_id=' + encodeURIComponent(config.clientId) +
            '&state=' + encodeURIComponent(uuid) +
            '&redirect_uri=' + encodeURIComponent(callbackUrl) +
            '&scope=' + encodeURIComponent(config.scope ? 'openid ' + config.scope : 'openid') +
            '&response_type=code'

        if (config && config.idpHint) {
            url += '&kc_idp_hint=' + encodeURIComponent(config.idpHint)
        }
        return url
    }

    logoutUrl(redirectUrl, idTokenHint) {
        const url = new URL(this.opt.realmUrl + '/protocol/openid-connect/logout')

        if (redirectUrl && idTokenHint) {
            url.searchParams.set('id_token_hint', idTokenHint)
            url.searchParams.set('post_logout_redirect_uri', redirectUrl)
        }
        return url.toString()
    }
}